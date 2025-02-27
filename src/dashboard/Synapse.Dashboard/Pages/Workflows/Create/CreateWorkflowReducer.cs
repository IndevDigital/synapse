﻿/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
using Neuroglia.Data.Flux;

namespace Synapse.Dashboard.Pages.Workflows.Create.Reducer
{
    [Reducer]
    public static class CreateWorkflowReducer
    {
        public static State.CreateWorkflowState OnToggleEditorVisibility(State.CreateWorkflowState state, Actions.ToggleVisualEditor action)
        {
            return state with
            {
                ShowVisualEditor = !state.ShowVisualEditor
            };
        }
        public static State.CreateWorkflowState OnEnableCreateButton(State.CreateWorkflowState state, Actions.EnableCreateButton action)
        {
            return state with
            {
                CreateDisabled = false
            };
        }
        public static State.CreateWorkflowState OnDisableCreateButton(State.CreateWorkflowState state, Actions.DisableCreateButton action)
        {
            return state with
            {
                CreateDisabled = true
            };
        }
    }
}
