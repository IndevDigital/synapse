﻿using Blazor.Diagrams.Core.Models;
using ServerlessWorkflow.Sdk.Models;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Represents a <see cref="SubflowReference"/> <see cref="NodeViewModel"/>
    /// </summary>
    public class SubflowRefNodeViewModel
        : WorkflowNodeViewModel
    {

        /// <summary>
        /// Initializes a new <see cref="SubflowRefNodeViewModel"/>
        /// </summary>
        /// <param name="subflow">The <see cref="SubflowReference"/> the <see cref="SubflowRefNodeViewModel"/> represents</param>
        public SubflowRefNodeViewModel(SubflowReference subflow)
            : base($"{subflow.WorkflowId}{(string.IsNullOrEmpty(subflow.Version) ? "" : $":{subflow.Version}")}")
        {
            this.Subflow = subflow;
        }

        /// <summary>
        /// Gets the <see cref="SubflowReference"/> the <see cref="SubflowRefNodeViewModel"/> represents
        /// </summary>
        public SubflowReference Subflow { get; }

    }

}
