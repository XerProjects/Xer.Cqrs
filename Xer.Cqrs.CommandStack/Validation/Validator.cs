using System;
using System.Collections.Generic;
using static Xer.Cqrs.CommandStack.Validation.ValidationRequirementRegistration;

namespace Xer.Cqrs.CommandStack.Validation
{
    public class Validator<TTarget> : IValidator<TTarget> where TTarget : class
    {
        private readonly ValidationRequirementRegistration _registration;

        public Validator(ValidationRequirementRegistration registration)
        {
            _registration = registration;
        }

        public virtual void Validate(TTarget target)
        {
            Type targetType = target.GetType();

            IEnumerable<ValidateRequirementDelegate> validateRequirementDelegates = _registration.GetRequirementValidators(targetType);

            foreach(ValidateRequirementDelegate validateRequirementDelegate in validateRequirementDelegates)
            {
                ValidationRequirementResult result = validateRequirementDelegate.Invoke(target);
                if (!result.IsRequirementSatisfied)
                {
                    throw new ValidationException(result.ErrorMessage);
                }
            }
        }
    }
}
