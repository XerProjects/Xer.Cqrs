using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Commands;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ReadSide.Products;
using ReadSide.Products.Queries;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.QueryStack;
using Xer.Delegator;

namespace AspNetCore.Controllers
{
    /// <summary>
    /// Products controller.
    /// </summary>
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly CommandDelegator _commandDelegator;
        private readonly IQueryAsyncDispatcher _queryDispatcher;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="commandDelegator">Command delegator.</param>
        /// <param name="queryDispatcher">Query dispatcher.</param>;
        public ProductsController(CommandDelegator commandDelegator, IQueryAsyncDispatcher queryDispatcher)
        {
            _commandDelegator = commandDelegator;
            _queryDispatcher = queryDispatcher;
        }

        // GET api/products
        /// <summary>
        /// Get all products.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            IReadOnlyCollection<ProductReadModel> products = await _queryDispatcher.DispatchAsync<QueryAllProducts, IReadOnlyCollection<ProductReadModel>>(new QueryAllProducts());
            
            return Ok(products ?? Enumerable.Empty<ProductReadModel>());
        }

        // GET api/products/{productId}
        /// <summary>
        /// Get a product by ID.
        /// </summary>
        /// <param name="productId">ID of product to retrieve.</param>
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(int productId)
        {
            ProductReadModel product = await _queryDispatcher.DispatchAsync<QueryProductById, ProductReadModel>(new QueryProductById(productId));
            if (product != null)
            {
                return Ok(product);
            }

            return NotFound();
        }

        // POST api/products
        /// <summary>
        /// Register a new product.
        /// </summary>
        /// <param name="model">Product model.</param>
        [HttpPost]
        public async Task<IActionResult> RegisterProduct([FromBody]RegisterProductCommandDto model)
        {
            RegisterProductCommand command = model.ToDomainCommand();
            
            await _commandDelegator.SendAsync(command);
            return Ok();
        }

        // PUT api/products/{productId}
        /// <summary>
        /// Modify product.
        /// </summary>
        /// <param name="productId">ID of the product.</param>
        /// <param name="operation">
        /// <para>Operation to perform to the product:</para>
        /// <para>Valid values are:</para>
        /// <para>- ActivateProduct</para>
        /// <para>- DeactivateProduct</para>
        /// </param>
        /// <param name="payload">JSON payload, if available.</param>
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

        /// <summary>
        /// Register product command DTO.
        /// </summary>
        public class RegisterProductCommandDto
        {
            /// <summary>
            /// Product ID.
            /// </summary>
            /// <returns></returns>
            public int ProductId { get; set; }

            /// <summary>
            /// Product name.
            /// </summary>
            /// <returns></returns>
            public string ProductName { get; set; }

            /// <summary>
            /// Translate DTO to domain command object.
            /// </summary>
            /// <returns>Domain RegisterProductCommand object.</returns>
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
