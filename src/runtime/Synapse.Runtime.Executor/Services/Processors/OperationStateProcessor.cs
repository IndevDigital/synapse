﻿/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Neuroglia.Serialization;
using Synapse.Integration.Events.WorkflowActivities;
using System.Reactive.Linq;

namespace Synapse.Runtime.Executor.Services.Processors
{

    /// <summary>
    /// Represents the <see cref="IWorkflowActivityProcessor"/> used to process <see cref="OperationStateDefinition"/>s
    /// </summary>
    public class OperationStateProcessor
        : StateProcessor<OperationStateDefinition>
    {

        /// <inheritdoc/>
        public OperationStateProcessor(ILoggerFactory loggerFactory, IWorkflowRuntimeContext context, IWorkflowActivityProcessorFactory activityProcessorFactory,
            IJsonSerializer jsonSerializer, IOptions<ApplicationOptions> options, V1WorkflowActivityDto activity, OperationStateDefinition state)
            : base(loggerFactory, context, activityProcessorFactory, options, activity, state)
        {
            this.JsonSerializer = jsonSerializer;
        }

        /// <summary>
        /// Gets the service used to serialize/deserialize to/from JSON
        /// </summary>
        protected IJsonSerializer JsonSerializer { get; }

        /// <inheritdoc/>
        protected override IWorkflowActivityProcessor CreateProcessorFor(V1WorkflowActivityDto activity)
        {
            var processor = (ActionProcessor)base.CreateProcessorFor(activity);
            processor.OfType<V1WorkflowActivityCompletedIntegrationEvent>().SubscribeAsync
            (
                async e => await this.OnActionNextAsync(processor, e, this.CancellationTokenSource.Token),
                async ex => await this.OnActionErrorAsync(processor, ex, this.CancellationTokenSource.Token),
                async () => await this.OnActionCompletedAsync(processor, this.CancellationTokenSource.Token)
            );
            return processor;
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (this.Activity.Status == V1WorkflowActivityStatus.Pending)
            {
                var input = null as object;
                var metadata = null as Dictionary<string, string>;
                switch (this.State.ActionMode)
                {
                    case ActionExecutionMode.Parallel:
                        foreach (var action in this.State.Actions)
                        {
                            input = this.Context.ExpressionEvaluator.FilterInput(action, this.Activity.Input);
                            metadata = new() 
                            {
                                { V1WorkflowActivityMetadata.State, this.State.Name! },
                                { V1WorkflowActivityMetadata.Action, action.Name! } 
                            };
                            await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Action, input, metadata, this.Activity, cancellationToken);
                        }
                        break;
                    case ActionExecutionMode.Sequential:
                        var firstAction = this.State.Actions.First();
                        input = this.Context.ExpressionEvaluator.FilterInput(firstAction, this.Activity.Input);
                        metadata = new()
                        {
                            { V1WorkflowActivityMetadata.State, this.State.Name! },
                            { V1WorkflowActivityMetadata.Action, firstAction.Name! }
                        };
                        await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Action, input, metadata, this.Activity, cancellationToken);
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(ActionExecutionMode)} '{this.State.ActionMode}' is not supported");
                }
            }
            foreach (var activity in await this.Context.Workflow.GetOperativeActivitiesAsync(this.Activity, cancellationToken))
            {
                _ = this.CreateProcessorFor(activity);
            }
        }

        /// <inheritdoc/>
        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            foreach (IWorkflowActivityProcessor processor in this.Processors.ToList())
            {
                await processor.ProcessAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Aggregates the output of the completed actions activities that belongs to the processed <see cref="OperationStateDefinition"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The result of the action outputs aggregation</returns>
        protected virtual async Task<object> AggregateActionOutputsAsync(CancellationToken cancellationToken)
        {
            var output = this.Activity.Input.ToObject();
            foreach (var activity in (await this.Context.Workflow.GetActivitiesAsync(this.Activity, cancellationToken))
                .Where(a => a.Type == V1WorkflowActivityType.Action && a.Status == V1WorkflowActivityStatus.Completed)
                .OrderBy(a => a.ExecutedAt))
            {
                if (!activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Action, out var actionName))
                    throw new ArgumentException($"The specified activity '{activity.Id}' is missing the required metadata field '{V1WorkflowActivityMetadata.Action}'");
                if (!this.State.TryGetAction(actionName, out var action))
                    throw new NullReferenceException($"Failed to find an action with name '{actionName}' in the operation state with name '{this.State.Name}'");
                if (action.UseResults())
                {
                    var expression = action.ActionDataFilter?.ToStateData?.Trim();
                    var json = await this.JsonSerializer.SerializeAsync(output, cancellationToken);
                    if (string.IsNullOrWhiteSpace(expression))
                    {
                        expression = json;
                    }
                    else
                    {
                        if (expression.StartsWith("${"))
                            expression = expression[2..^1];
                        expression = $"{expression} = {json}";
                    }
                    output = this.Context.ExpressionEvaluator.Evaluate(expression, activity.Output.ToObject())!;
                }
            }
            return output;
        }

        /// <summary>
        /// Handles <see cref="V1WorkflowActivityDto"/>s <see cref="V1WorkflowActivityCompletedIntegrationEvent"/>
        /// </summary>
        /// <param name="processor">The <see cref="IActionProcessor"/> to handle the <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> for</param>
        /// <param name="e">The <see cref="V1WorkflowActivityCompletedIntegrationEvent"/> to handle</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActionNextAsync(IActionProcessor processor, V1WorkflowActivityCompletedIntegrationEvent e, CancellationToken cancellationToken)
        {
            using (await this.Lock.LockAsync(cancellationToken))
            {
                var output = null as object;
                switch (this.State.ActionMode)
                {
                    case ActionExecutionMode.Parallel:
                        var activities = (await this.Context.Workflow.GetActivitiesAsync(this.Activity, cancellationToken))
                           .Where(a => a.Type == V1WorkflowActivityType.Action)
                           .ToList();
                        if (activities.All(a => a.Status == V1WorkflowActivityStatus.Completed))
                        {
                            output = await this.AggregateActionOutputsAsync(cancellationToken);
                            await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
                        }
                        break;
                    case ActionExecutionMode.Sequential:
                        output = await this.AggregateActionOutputsAsync(cancellationToken);
                        if (processor.Activity.Metadata.TryGetValue(V1WorkflowActivityMetadata.Action, out var currentActionName)
                             && this.State.TryGetNextAction(currentActionName, out ActionDefinition nextAction))
                        {
                            var input = this.Context.ExpressionEvaluator.FilterInput(nextAction, output);
                            var metadata = new Dictionary<string, string>()
                            {
                                { V1WorkflowActivityMetadata.State, this.State.Name! },
                                { V1WorkflowActivityMetadata.Action, nextAction.Name! }
                            };
                            await this.Context.Workflow.CreateActivityAsync(V1WorkflowActivityType.Action, input, metadata, this.Activity, cancellationToken);
                        }
                        else
                        {
                            await this.OnNextAsync(new V1WorkflowActivityCompletedIntegrationEvent(this.Activity.Id, output), cancellationToken);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"The specified {nameof(ActionExecutionMode)} '{this.State.ActionMode}' is not supported");
                }
            }
        }

        /// <summary>
        /// Handles an <see cref="ActionDefinition"/>'s <see cref="Exception"/>
        /// </summary>
        /// <param name="processor">The <see cref="ActionProcessor"/> that has thrown the specified <see cref="Exception"/></param>
        /// <param name="ex">The thrown <see cref="Exception"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActionErrorAsync(ActionProcessor processor, Exception ex, CancellationToken cancellationToken)
        {
            using (await this.Lock.LockAsync(cancellationToken))
            {
                await base.OnErrorAsync(ex, cancellationToken);
            }
        }

        /// <summary>
        /// Handles the completion of the specified <see cref="V1WorkflowActivity"/>
        /// </summary>
        /// <param name="processor">The <see cref="ActionProcessor"/> that has returned the <see cref="V1WorkflowExecutionResult"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task OnActionCompletedAsync(ActionProcessor processor, CancellationToken cancellationToken)
        {
            using (await this.Lock.LockAsync(cancellationToken))
            {
                this.Processors.TryRemove(processor);
                processor.Dispose();
                var activities = (await this.Context.Workflow.GetActivitiesAsync(this.Activity, cancellationToken))
                    .Where(a => a.Type == V1WorkflowActivityType.Action);
                if (activities.All(a => a.Status == V1WorkflowActivityStatus.Completed))
                {
                    await base.OnCompletedAsync(cancellationToken);
                    return;
                }
                foreach (var activity in activities
                    .Where(p => p.Status == V1WorkflowActivityStatus.Pending))
                {
                    var nextProcessor = this.CreateProcessorFor(activity);
                    await nextProcessor.ProcessAsync(cancellationToken);
                }
            }
        }

    }

}
