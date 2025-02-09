﻿namespace WixServer.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Dtos;
    using Models;
    using System.Data.Entity.Infrastructure;
    using System.Net;

    public class GridsController : ApiController
    {
        private WixServerContext db = new WixServerContext();

        // GET: api/Grids
        public IQueryable<Grid> GetGrids()
        {
            return db.Grids;
        }

        [Route("api/Grids/{restaurantId}/{date}")]
        [ResponseType(typeof(GridDto))]
        public IHttpActionResult GetGrid(int restaurantId,string date)
        {
            // TODO: change the signature of this mathod to be meaningful
            //userName = "sergey@gmail.com";
            //password = "Aa123456";

            //var restaurantOwner = db.RestaurantOwners.Where(x => x.UserName == userName && x.Password == password).FirstOrDefault();

            //if (restaurantOwner == null)
            //{
            //    return NotFound();
            //}

            //var restaurantId = restaurantOwner.RestaurantId;

            var today = Convert.ToDateTime(date);

            var grid = db.Grids.Where(x => DbFunctions.TruncateTime(x.Date).Value == today && x.RestaurantId == restaurantId).FirstOrDefault();
            //var grid = db.Grids.Where(x => x.RestaurantId == restaurantId).FirstOrDefault();

            if (grid == null)
            {
                return NotFound();
            }

            var tables = db.Tables.Where(x => x.GridId == grid.Id).ToList();

            var gridItems = db.GridItems.Where(x => x.GridId == grid.Id).ToList();

            var orders = db.Orders.Where(x => x.GridId == grid.Id).ToList();

            GridDto gridDto = new GridDto();

            gridDto.simpleItems = gridItems;

            gridDto.XLen = grid.XLen;
            gridDto.YLen = grid.YLen;
            gridDto.Id = grid.Id;

            gridDto.Items = new List<ItemDto>();

            tables.ForEach(table =>
            {
                var tableDto = new GridTableDto
                {
                    X = table.xCoord,
                    Y = table.yCoord,
                    MaxCapacity = table.Capacity,
                    SmokingAllowed = table.IsSmokingAllowed,
                    TableNumber = table.TableNumber,
                    XLen = table.xLength,
                    YLen = table.yLength
                };

                tableDto.Taken = orders.Where(x => x.TableNumber == tableDto.TableNumber).FirstOrDefault() != null;

                gridDto.Items.Add(tableDto);
            });

            return Ok(gridDto);
        }

        // POST: api/Grids
        [Route("api/Grids/{restaurantId}/{date}/{gridType}/{name}/{isDefault}/{xlen}/{ylen}")]
        [ResponseType(typeof(int))]
        public IHttpActionResult PostGrid(int restaurantId, string date, int gridType, string name, bool isDefault, int xlen, int ylen)
        {
            //var gridToDelete = db.Grids.Where(x => x.RestaurantId == 10).FirstOrDefault();
            //if (gridToDelete != null)
            //{
            //    db.Grids.Remove(gridToDelete);
            //    db.SaveChanges();
            //}

            // Retrieve the max id
            var id = db.Grids.Max(x => x.Id) + 1;

            // To transfer the parameters to a grid
            //Grid grid = new Grid
            //{
            //    Id = id,
            //    RestaurantId = restaurantId,
            //    Date = DateTime.FromBinary(date),
            //    GridType = gridType,
            //    Name = name,
            //    IsDefault = isDefault,
            //    XLen = xlen,
            //    YLen = ylen
            //};

            Grid grid = new Grid
            {
                Id = id,
                RestaurantId = restaurantId,
                Date = Convert.ToDateTime(date),
                GridType = gridType,
                Name = name,
                IsDefault = isDefault,
                XLen = xlen,
                YLen = ylen
            };

            try
            {
                db.Grids.Add(grid);

                db.SaveChanges();
            }
            catch (Exception e)
            {
                //if (!GridExists(id))
                //{
                //    return NotFound();
                //}
                //else
                //{
                //    throw;
                //}
            }

            return Ok(grid.Id);
        }

        // DELETE: api/Grids/5
        [ResponseType(typeof(Grid))]
        public IHttpActionResult DeleteGrid(int id)
        {
            Grid grid = db.Grids.Find(id);
            if (grid == null)
            {
                return NotFound();
            }

            db.Grids.Remove(grid);
            db.SaveChanges();

            return Ok(grid);
        }

        [ResponseType(typeof(void))]
        public IHttpActionResult PutGrid(int id, Grid grid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != grid.Id)
            {
                return BadRequest();
            }

            db.Entry(grid).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GridExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GridExists(int id)
        {
            return db.Grids.Count(e => e.Id == id) > 0;
        }
    }
}