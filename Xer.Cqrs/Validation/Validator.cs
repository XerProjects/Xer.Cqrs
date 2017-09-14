using System;
using System.Collections.Generic;
using System.Text;
using Xer.Cqrs.Exceptions;
using static Xer.Cqrs.Validation.ValidationRequirementRegistration;

namespace Xer.Cqrs.Validation
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
