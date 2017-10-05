using System;
using System.Collections.Generic;
using System.Linq;

namespace Xer.Cqrs.CommandStack.Validation
{
    public class ValidationRequirementRegistration
    {
        private readonly Dictionary<Type, ICollection<ValidateRequirementDelegate>> _requirementValidationByCommandType = new Dictionary<Type, ICollection<ValidateRequirementDelegate>>();

        /// <summary>
        /// Add a command specification to this validator instance.
        /// </summary>
        /// <typeparam name="TTarget">Type of object which the requirement is for.</typeparam>
        /// <param name="requirement">Validation requirement.</param>
        public void RegisterRequirement<TTarget>(IValidationRequirement<TTarget> requirement) where TTarget : class
        {
            Type commandType = typeof(TTarget);

            addRequirement(commandType, requirement);
        }

        internal IEnumerable<ValidateRequirementDelegate> GetRequirementValidators(Type targetType)
        {
            ICollection<ValidateRequirementDelegate> requirementValidationFuncs;

            if(!_requirementValidationByCommandType.TryGetValue(targetType, out requirementValidationFuncs))
            {
                return Enumerable.Empty<ValidateRequirementDelegate>();
            }

            return requirementValidationFuncs;
        }

        /// <summary>
        /// Add command specification.
        /// </summary>
        /// <typeparam name="TTarget">Type of object which the requirement is for.</typeparam>
        /// <param name="targetType">Type of object which the requirement is for.</param>
        /// <param name="requirement">Validation requirement.</param>
        private void addRequirement<TTarget>(Type targetType, IValidationRequirement<TTarget> requirement) where TTarget : class
        {
            ValidateRequirementDelegate validateRequirementDelegate = (obj) =>
            {
                bool isSatisfied = requirement.IsSatisfiedBy((TTarget)obj);
                return new ValidationRequirementResult(isSatisfied, requirement.ErrorMessage);
            };

            ICollection<ValidateRequirementDelegate> requirementValidationFuncs;

            if (_requirementValidationByCommandType.TryGetValue(targetType, out requirementValidationFuncs))
            {
                requirementValidationFuncs.Add(validateRequirementDelegate);
            }
            else
            {
                _requirementValidationByCommandType.Add(targetType,
                    new List<ValidateRequirementDelegate> { validateRequirementDelegate });
            }
        }

        /// <summary>
        /// Validate requirement delegate.
        /// </summary>
        /// <param name="target">Object to validate.</param>
        /// <returns>Requirement validation result.</returns>
        internal delegate ValidationRequirementResult ValidateRequirementDelegate(object target);

        /// <summary>
        /// Internal representation of spec validation result.
        /// </summary>
        internal class ValidationRequirementResult
        {
            public bool IsRequirementSatisfied { get; private set; }
            public string ErrorMessage { get; private set; }

            public ValidationRequirementResult(bool isRequirementSatisfied, string errorMessage)
            {
                IsRequirementSatisfied = isRequirementSatisfied;
                ErrorMessage = errorMessage ?? string.Empty;
            }
        }
    }
}
