﻿
/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0(the "License");
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
 */

/* -----------------------------------------------------------------------
 * This file has been automatically generated by a tool
 * -----------------------------------------------------------------------
 */

namespace Synapse.Integration.Models
{

	/// <summary>
	/// Represents a workflow
	/// </summary>
	[DataContract]
	[Queryable]
	public partial class V1Workflow
		: Entity
	{

		/// <summary>
		/// The V1Workflow's definition
		/// </summary>
		[DataMember(Name = "Definition", Order = 1)]
		[Description("The V1Workflow's definition")]
		public virtual WorkflowDefinition Definition { get; set; }

		/// <summary>
		/// The date and time at which the V1Workflow was last instanciated
		/// </summary>
		[DataMember(Name = "LastInstanciated", Order = 2)]
		[Description("The date and time at which the V1Workflow was last instanciated")]
		public virtual DateTime? LastInstanciated { get; set; }

    }

}
