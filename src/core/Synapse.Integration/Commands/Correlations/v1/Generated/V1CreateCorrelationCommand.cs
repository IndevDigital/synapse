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

namespace Synapse.Integration.Commands.Correlations
{

	/// <summary>
	/// Represents the ICommand used to create a new V1Correlation
	/// </summary>
	[DataContract]
	public partial class V1CreateCorrelationCommand
		: Command
	{

		/// <summary>
		/// The lifetime of the V1Correlation to create
		/// </summary>
		[DataMember(Name = "Lifetime", Order = 1)]
		[Description("The lifetime of the V1Correlation to create")]
		[Required]
		public virtual V1CorrelationLifetime Lifetime { get; set; }

		/// <summary>
		/// The type of V1CorrelationCondition evaluation the V1Correlation should use
		/// </summary>
		[DataMember(Name = "ConditionType", Order = 2)]
		[Description("The type of V1CorrelationCondition evaluation the V1Correlation should use")]
		[Required]
		public virtual V1CorrelationConditionType ConditionType { get; set; }

		/// <summary>
		/// An IEnumerable`1 containing all V1CorrelationConditions the V1Correlation to create is made out of
		/// </summary>
		[DataMember(Name = "Conditions", Order = 3)]
		[Description("An IEnumerable`1 containing all V1CorrelationConditions the V1Correlation to create is made out of")]
		[MinLength(1)]
		public virtual IEnumerable<V1CorrelationCondition> Conditions { get; set; }

		/// <summary>
		/// The V1CorrelationOutcome of the V1Correlation to create
		/// </summary>
		[DataMember(Name = "Outcome", Order = 4)]
		[Description("The V1CorrelationOutcome of the V1Correlation to create")]
		[Required]
		public virtual V1CorrelationOutcome Outcome { get; set; }

		/// <summary>
		/// The initial V1CorrelationContext of the V1Correlation to create
		/// </summary>
		[DataMember(Name = "Context", Order = 5)]
		[Description("The initial V1CorrelationContext of the V1Correlation to create")]
		public virtual V1CorrelationContext Context { get; set; }

    }

}
