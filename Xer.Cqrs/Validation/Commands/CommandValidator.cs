using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xer.Cqrs.Exceptions;

namespace Xer.Cqrs.Validation.Commands
{
    public class CommandValidator : ICommandValidator
    {
        private IDictionary<Type, IEnumerable<Func<ICommand, CommandValidationResult>>> _requirementValidationByCommandType = new Dictionary<Type, IEnumerable<Func<ICommand, CommandValidationResult>>>();

        /// <summary>
        /// Add a command specification to this validator instance.
        /// </summary>
        /// <typeparam name="TCommand">Type of command which the specification is for.</typeparam>
        /// <param name="requirement">Command requirement.</param>
        void IValidator<ICommand>.AddRequirement<TCommand>(IRequirement<TCommand> requirement)
        {
            Type commandType = typeof(TCommand);

            if (!typeof(ICommand).GetTypeInfo().IsAssignableFrom(commandType))
            {
                throw new ArgumentException("Requirement is not for ICommand types.", nameof(requirement));
            }

            addRequirement(commandType, requirement);
        }

        /// <summary>
        /// Add a command specification to this validator instance.
        /// </summary>
        /// <typeparam name="TCommand">Type of command which the specification is for.</typeparam>
        /// <param name="specification">Command specification.</param>
        public void AddRequirement<TCommand>(IRequirement<TCommand> specification) where TCommand : ICommand
        {
            Type commandType = typeof(TCommand);

            addRequirement(commandType, specification);
        }

        /// <summary>
        /// Validate command by looking for applicable specifications.
        /// </summary>
        /// <param name="command">Command to validate.</param>
        public void Validate(ICommand command)
        {
            Type commandType = command.GetType();

            IEnumerable<Func<ICommand, CommandValidationResult>> requirementValidationFuncs;

            if(_requirementValidationByCommandType.TryGetValue(commandType, out requirementValidationFuncs))
            {
                foreach (Func<ICommand, CommandValidationResult> requirementValidationFunc in requirementValidationFuncs)
                {
                    CommandValidationResult result = requirementValidationFunc.Invoke(command);
                    if (!result.IsRequirementSatisfied)
                    {
                        throw new ValidationException(result.ErrorMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Add command specification.
        /// </summary>
        /// <typeparam name="TCommand">Type of command which the specification is for.</typeparam>
        /// <param name="commandType">Type of command which the specification is for.</param>
        /// <param name="requirement">Command requirement.</param>
        private void addRequirement<TCommand>(Type commandType, IRequirement<TCommand> requirement)
        {
            IEnumerable<Func<ICommand, CommandValidationResult>> requirementValidationFuncs;

            if (_requirementValidationByCommandType.TryGetValue(commandType, out requirementValidationFuncs))
            {
                _requirementValidationByCommandType[commandType].Append(new Func<ICommand, CommandValidationResult>((c) =>
                {
                    var requirementIsSatisfied = requirement.IsSatisfiedBy((TCommand)c);
                    return new CommandValidationResult(requirementIsSatisfied, requirement.ErorMessage);
                }));
            }
            else
            {
                _requirementValidationByCommandType.Add(commandType, new List<Func<ICommand, CommandValidationResult>>
                {
                    new Func<ICommand, CommandValidationResult>((c) =>
                    {
                        var requirementIsSatisfied = requirement.IsSatisfiedBy((TCommand)c);
                        return new CommandValidationResult(requirementIsSatisfied, requirement.ErorMessage);
                    })
                });
            }
        }

        /// <summary>
        /// Internal representation of spec validation result.
        /// </summary>
        private class CommandValidationResult
        {
            public bool IsRequirementSatisfied { get; private set; }
            public string ErrorMessage { get; private set; }

            public CommandValidationResult(bool isRequirementSatisfied, string errorMessage)
            {
                IsRequirementSatisfied = isRequirementSatisfied;
                ErrorMessage = errorMessage ?? string.Empty;
            }
        }
    } 
}
