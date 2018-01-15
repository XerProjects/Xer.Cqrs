using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Commands;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Xer.Cqrs.CommandStack;
using Xer.Delegator;

namespace AspNetCore.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly IMessageDelegator _commandDelegator;
        private readonly IProductRepository _productRepository;
        
        public ProductsController(IMessageDelegator commandDispatcher, IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _commandDelegator = commandDispatcher;
        }

        // GET api/products/{productId}
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(int productId)
        {
            Product product = await _productRepository.GetProductByIdAsync(productId);
            if(product != null)
            {
                return Ok(product);
            }

            return NotFound();
        }

        // POST api/products
        [HttpPost]
        public async Task<IActionResult> RegisterProduct([FromBody]RegisterProductCommandDto model)
        {
            RegisterProductCommand command = model.ToDomainCommand();
            
            await _commandDelegator.SendAsync(command);
            return Ok();
        }

        // PUT api/products/{productId}
        [HttpPut("{productId}")]
        public Task<IActionResult> ModifyProduct(int productId, [FromHeader]string operation, [FromBody]JObject payload)
        {
            switch(operation)
            {
                case ProductOperations.ActivateProduct:
                    return InternalActivateProduct(productId);
                case ProductOperations.DeactivateProduct:
                    return InternalDeactivateProduct(productId);
                default:
                    return Task.FromResult<IActionResult>(BadRequest());
            }
        }

        private async Task<IActionResult> InternalActivateProduct(int productId)
        {
            await _commandDelegator.SendAsync(new ActivateProductCommand(productId));
            return Ok();
        }

        private async Task<IActionResult> InternalDeactivateProduct(int productId)
        {
            await _commandDelegator.SendAsync(new DeactivateProductCommand(productId));
            return Ok();
        }

        public class RegisterProductCommandDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }

            public RegisterProductCommand ToDomainCommand()
            {
                return new RegisterProductCommand(ProductId, ProductName);
            }
        }

        class ProductOperations
        {
            public const string ActivateProduct = nameof(ActivateProduct);
            public const string DeactivateProduct = nameof(DeactivateProduct);
        }
    }
}
