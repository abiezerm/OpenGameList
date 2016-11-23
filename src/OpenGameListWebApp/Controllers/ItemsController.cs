using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OpenGameListWebApp.ViewModels;
using Newtonsoft.Json;
using System.Linq;
using Nelibur.ObjectMapper;
using OpenGameListWebApp.Data;
using OpenGameListWebApp.Data.Items;

namespace OpenGameListWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {
        private int MaxNumberOfItems => 100;
        private int DefaultNumberOfItems => 5;
        private readonly ApplicationDbContext _dbContext;

        public ItemsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        ///<summary>
        /// GET: api/items
        /// </summary>
        /// <returns>
        /// Nothing: this method will raise a HttpNotFound HTTPException,
        /// since we're not supporting this API call.
        /// </returns>
        [HttpGet()]
        public IActionResult Get()
        {
            return NotFound(new
            {
                Error = "not found"
            });
        }

        /// <summary> 
        /// GET: api/items/{id} 
        ///  ROUTING TYPE: attribute-based 
        /// </summary> 
        /// <returns>
        /// A Json-serialized object representing a single item.
        /// </returns> 
        [HttpGet("{id}")] public IActionResult Get(int id)
        {
            var item = _dbContext.Items.FirstOrDefault(i => i.Id == id);

            if(item == null) return NotFound(new { Error = $"Item ID {id} has not been found" });

            return new JsonResult(TinyMapper.Map<ItemViewModel>(item), DefaultJsonSettings);
        }

        ///<summary>
        /// POST: api/items
        /// </summary>
        /// <returns>
        /// Creates a new Item and return it accordingly.
        /// </returns>
        [HttpPost]
        public IActionResult Add([FromBody] ItemViewModel ivm)
        {
            if(ivm == null) return new StatusCodeResult(500);

            var item = TinyMapper.Map<Item>(ivm);
            item.CreatedDate = item.LastModifiedDate = DateTime.Now;
            item.UserId = _dbContext.Users.FirstOrDefault(u => u.UserName == "Admin")?.Id;

            _dbContext.Items.Add(item);
            _dbContext.SaveChanges();

            return new JsonResult(TinyMapper.Map<ItemViewModel>(item), DefaultJsonSettings);
        }

        ///<summary>
        /// PUT: api/items/{id}
        /// </summary>
        /// <returns>
        /// Updates an existing Item and return accordingly
        /// </returns>
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] ItemViewModel ivm)
        {
            var item = _dbContext.Items.FirstOrDefault(i => i.Id == id);

            if (item == null) return NotFound(new {Error = $"Item ID {id} has not been found"});

            item.UserId = ivm.UserId;
            item.Description = ivm.Description;
            item.Flags = ivm.Flags;
            item.Notes = ivm.Notes;
            item.Text = ivm.Text;
            item.Title = ivm.Title;
            item.Type = ivm.Type;

            item.LastModifiedDate = DateTime.Now;

            _dbContext.SaveChanges();
            return new JsonResult(TinyMapper.Map<ItemViewModel>(item), DefaultJsonSettings);
        }

        ///<summary>
        /// DELETE: api/items/{id}
        /// </summary>
        /// <returns>
        /// Deletes an item returning a HTTP status 200 (ok) when done.
        /// </returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var item = _dbContext.Items.FirstOrDefault(i => i.Id == id);

            if (item == null) return NotFound(new { Error = $"Item ID {id} has not been found" });

            _dbContext.Items.Remove(item);

            _dbContext.SaveChanges();

            return new OkResult();
        }

        /// <summary> 
        /// GET: api/items/GetLatest /// 
        /// ROUTING TYPE: attribute-based /// 
        /// </summary> /// 
        /// <returns>
        /// An array of a default number of Json-serialized objects 
        /// representing the last inserted items.
        /// </returns> 
        [HttpGet("GetLatest")]
        public IActionResult GetLatest()
        {
            return GetLatest(DefaultNumberOfItems);
        } 

        /// <summary> 
        /// GET: api/items/GetLatest/{n} 
        /// ROUTING TYPE: attribute-based 
        /// </summary> 
        /// <returns>An array of {n} Json-serialized objects representing the last inserted items.</returns>
        [HttpGet("GetLatest/{n}")]
        public JsonResult GetLatest(int n)
        {
            if (n > MaxNumberOfItems)
                n = MaxNumberOfItems;

            var items = _dbContext.Items.OrderByDescending(i => i.CreatedDate)
                .Take(n).ToArray();

            return new JsonResult(ToItemViewModelList(items), DefaultJsonSettings);
        }

        ///<summary>
        /// GET: api/items/GetMostViewed
        /// ROUTING TYPE: attribure based
        /// </summary>
        /// <returns>
        /// An array of a default number of json serialized objects representing
        /// the items with most user views.
        /// </returns>
        [HttpGet("GetMostViewed")]
        public IActionResult GetMostviewed()
        {
            return GetMostViewed(DefaultNumberOfItems);
        }

        /// <summary> 
        /// GET: api/items/GetMostViewed/{n} 
        /// ROUTING TYPE: attribute-based 
        /// </summary> 
        /// <returns>An array of {n} Json-serialized objects representing the items with most user views.</returns> 
        [HttpGet("GetMostViewed/{n}")]
        public IActionResult GetMostViewed(int n)
        {
            if (n > MaxNumberOfItems)
                n = MaxNumberOfItems;

            var items = _dbContext.Items.OrderByDescending(i => i.ViewCount)
                .Take(n).ToArray();

            return new JsonResult(ToItemViewModelList(items), DefaultJsonSettings);
        }

        ///<summary>
        /// GET: api/items/GetRandom
        /// ROUTING TYPE: attribure-based
        /// </summary>
        /// <returns>
        /// An array of a default number of json serialized objects representing
        /// some randomly-picked items.
        /// </returns>
        [HttpGet("GetRandom")]
        public IActionResult GetRandom()
        {
            return GetRandom(DefaultNumberOfItems);
        }

        /// <summary>
        /// GET: api/items/GetRandom/{n} 
        /// ROUTING TYPE: attribute-based 
        /// </summary> 
        /// <returns>An array of {n} Json-serialized objects representing some randomly-picked items.</returns> 
        [HttpGet("GetRandom/{n}")]
        public IActionResult GetRandom(int n)
        {
            if (n > MaxNumberOfItems)
                n = MaxNumberOfItems;

            var items = _dbContext.Items
                .OrderBy(i => Guid.NewGuid())
                .Take(n)
                .ToArray();

            return new JsonResult(ToItemViewModelList(items), DefaultJsonSettings);
        }

        private List<ItemViewModel> ToItemViewModelList(IEnumerable<Item> items)
        {
            return items.Select(i => TinyMapper.Map<ItemViewModel>(i)).ToList();
        }

        /// <summary>
        /// Returns a suitable JsonSerializerSettings object that can be used to generate the JsonResult return value for this Controller's methods. /// 
        /// </summary> 
        private JsonSerializerSettings DefaultJsonSettings => 
            new JsonSerializerSettings() { Formatting = Formatting.Indented };
    }
}