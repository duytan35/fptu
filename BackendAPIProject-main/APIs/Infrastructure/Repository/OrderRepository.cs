﻿using Application.InterfaceRepository;
using Application.InterfaceService;
using Application.ViewModel.OrderModel;
using Application.ViewModel.PostModel;
using Application.ViewModel.OrderModel;
using Application.ViewModel.UserModel;
using Dapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly IDbConnection _connection;
        private readonly AppDbContext _dbContext;
        public OrderRepository(AppDbContext appDbContext, IClaimService claimService
            , ICurrentTime currentTime, IDbConnection connection) : base(appDbContext, claimService, currentTime)
        {
            _connection = connection;
            _dbContext = appDbContext;
        }

        public async Task<List<ReceiveOrderViewModel>> GetAllOrder()
        {
            var listOrder = await _dbContext.Orders.Where(x => x.IsDelete == false)
                                            .Include(x => x.User).ThenInclude(u => u.VerifyUser).AsSplitQuery()
                                            .Include(x => x.User).ThenInclude(u => u.Raters).AsSplitQuery()
                                            .Include(x => x.Post).AsSplitQuery()
                                            .Include(x => x.Status).AsSplitQuery()
                                            .Select(x => new ReceiveOrderViewModel
                                            {
                                                OrderId = x.Id,
                                                OrderMessage = x.OrderMessage,
                                                OrderStatus = x.Status.StatusName,
                                                CreationDate = x.CreationDate,
                                                Post = new PostViewModelForOrder
                                                {
                                                    PostId = x.PostId,
                                                    PostContent = x.Post.PostContent,
                                                    PostTitle = x.Post.PostTitle
                                                },
                                                User = _dbContext.Users.Where(u => u.Id == x.UserId).AsSplitQuery().Select(u => new UserViewModelForOrder
                                                {
                                                    SenderId = x.CreatedBy.Value,
                                                    SenderEmail = u.Email,
                                                    SenderHomeAddress = u.HomeAddress,
                                                    SenderImageUrl = u.VerifyUser.UserImage,
                                                    SenderRating = (u.RatedUsers.Count() > 0
                                                                 ? u.RatedUsers.Sum(r => r.RatingPoint) / (u.RatedUsers.Count()) : 0),
                                                    SenderUsername = u.UserName
                                                }).Single()
                                            }).AsQueryable().AsNoTracking().OrderByDescending(x => x.CreationDate).ToListAsync();
            return listOrder;
        }

        public async Task<List<SentOrderViewModel>> GetAllOrderByCreatedByUserId(Guid userId)
        {
            var listOrder = await _dbContext.Orders.Where(x => x.IsDelete == false && x.CreatedBy == userId)
                                            .Include(x => x.User).ThenInclude(u => u.VerifyUser).AsSplitQuery()
                                            .Include(x => x.User).ThenInclude(u => u.Raters).AsSplitQuery()
                                            .Include(x => x.Post).AsSplitQuery()
                                            .Include(x => x.Status).AsSplitQuery()
                                            .Select(x => new SentOrderViewModel
                                            {
                                                OrderId = x.Id,
                                                OrderMessage = x.OrderMessage,
                                                OrderStatus=x.Status.StatusName,
                                                CreationDate = x.CreationDate,
                                                Post = new PostViewModelForOrder
                                                {
                                                    PostId = x.PostId,
                                                    PostContent = x.Post.PostContent,
                                                    PostTitle = x.Post.PostTitle
                                                },
                                                User = _dbContext.Users.Where(u => u.Id == x.UserId).AsSplitQuery().Select(u => new UserViewModelForOrder
                                                {
                                                    SenderId = x.CreatedBy.Value,
                                                    SenderEmail = u.Email,
                                                    SenderHomeAddress = u.HomeAddress,
                                                    SenderImageUrl = u.VerifyUser.UserImage,
                                                    SenderRating = (u.RatedUsers.Count() > 0
                                                                 ? u.RatedUsers.Sum(r => r.RatingPoint) / (u.RatedUsers.Count()) : 0),
                                                    SenderUsername = u.UserName
                                                }).Single()
                                            }).AsQueryable().AsNoTracking().OrderByDescending(x => x.CreationDate).ToListAsync();
            return listOrder;
        }

        public async Task<List<ReceiveOrderViewModel>> GetAllOrderByCurrentUserId(Guid userId)
        {
            /* var sql = @"
         SELECT 
             r.Id AS OrderId,
             r.OrderMessage,
             s.StatusName AS OrderStatus,
             u.Id AS SenderId,
             u.Email AS SenderEmail,
             u.UserName AS SenderUsername,
             p.Id AS PostId,
             p.PostContent,
             p.PostTitle
         FROM 
             Orders r
         INNER JOIN 
             Users u ON r.UserId = u.Id
         INNER JOIN 
             Posts p ON r.PostId = p.Id
         INNER JOIN 
             OrderStatuses s ON r.OrderStatusId = s.StatusId
         WHERE 
             r.UserId = @UserId;";

             var result = await _connection.QueryAsync<OrderViewModel, UserViewModelForOrder, PostViewModelForOrder, OrderViewModel>(
                 sql,
                 (Order, user, post) =>
                 {
                     Order.User = user;
                     Order.Post = post;
                     return Order;
                 },
                 new { UserId = userId },
                 splitOn: "SenderId, PostId"
             );

             return result.ToList();*/
            var listOrder = await _dbContext.Orders
                                             .Include(x => x.User).ThenInclude(u => u.VerifyUser).AsSplitQuery()
                                             .Include(x=>x.User).ThenInclude(u=>u.Raters).AsSplitQuery()
                                             .Include(x => x.Post).ThenInclude(p=>p.Product).ThenInclude(p=>p.Category).AsSplitQuery()
                                             .Include(x => x.Post).ThenInclude(p => p.Product).ThenInclude(p => p.ConditionType).AsSplitQuery()
                                             .Include(x => x.Status).AsSplitQuery()
                                             .Where(x => x.IsDelete == false && x.Post.CreatedBy == userId)
                                             .Select(x => new ReceiveOrderViewModel
                                             {
                                                 OrderId = x.Id,
                                                 OrderMessage = x.OrderMessage,
                                                 OrderStatus=x.Status.StatusName,
                                                 CreationDate = x.CreationDate,
                                                 Post = new PostViewModelForOrder
                                                 {
                                                     PostId = x.PostId,
                                                     PostContent = x.Post.PostContent,
                                                     PostTitle = x.Post.PostTitle,
                                                     Product = new ProductModel
                                                     {
                                                         CategoryId = x.Post.Product.CategoryId,
                                                         CategoryName = x.Post.Product.Category.CategoryName,
                                                         ConditionId = x.Post.Product.ConditionId,
                                                         ConditionName = x.Post.Product.ConditionType.ConditionType,
                                                         ProductId = x.Post.Product.Id,
                                                         ProductImageUrl = x.Post.Product.ProductImageUrl,
                                                         ProductPrice = x.Post.Product.ProductPrice,
                                                         ProductStatus = x.Post.Product.ProductStatus,
                                                         RequestedProduct = x.Post.Product.RequestedProduct
                                                     }
                                                     
                                                 },
                                                 User = _dbContext.Users.Where(u => u.Id == x.CreatedBy).AsSplitQuery().Select(u => new UserViewModelForOrder
                                                 {
                                                     SenderId = x.CreatedBy.Value,
                                                     SenderEmail = u.Email,
                                                     SenderHomeAddress = u.HomeAddress,
                                                     SenderImageUrl = u.ProfileImage,
                                                     SenderRating = (u.RatedUsers.Count() > 0
                                                                  ? u.RatedUsers.Sum(r => r.RatingPoint) / (u.RatedUsers.Count()): 0),
                                                     SenderUsername=u.UserName
                                                 }).Single()
                                             }).AsQueryable().AsNoTracking().OrderByDescending(x => x.CreationDate).ToListAsync();
            return listOrder;
        }

        public async Task<ReceiveOrderViewModel> GetOrderDetail(Guid orderId)
        {
            var detail = await _dbContext.Orders.Where(x => x.Id == orderId && x.IsDelete == false)
                                              .Include(x => x.User).ThenInclude(u => u.Raters).AsSplitQuery()
                                              .Include(x => x.Post).ThenInclude(p => p.Product).ThenInclude(p => p.Category).AsSplitQuery()
                                              .Include(x => x.Post).ThenInclude(p => p.Product).ThenInclude(p => p.ConditionType).AsSplitQuery()
                                              .Include(x => x.Status).AsSplitQuery()
                                              .Select(x => new ReceiveOrderViewModel
                                              {
                                                  OrderId=orderId,
                                                  OrderMessage=x.OrderMessage,
                                                  OrderStatus=x.Status.StatusName,
                                                  CreationDate=x.CreationDate,
                                                  Post=new PostViewModelForOrder
                                                  {
                                                      PostId=x.PostId,
                                                      PostContent=x.Post.PostContent,
                                                      PostTitle=x.Post.PostTitle,
                                                      Product=new ProductModel
                                                      {
                                                          CategoryId = x.Post.Product.CategoryId,
                                                          CategoryName = x.Post.Product.Category.CategoryName,
                                                          ConditionId = x.Post.Product.ConditionId,
                                                          ConditionName = x.Post.Product.ConditionType.ConditionType,
                                                          ProductId = x.Post.Product.Id,
                                                          ProductImageUrl = x.Post.Product.ProductImageUrl,
                                                          ProductPrice = x.Post.Product.ProductPrice,
                                                          ProductStatus = x.Post.Product.ProductStatus,
                                                          RequestedProduct = x.Post.Product.RequestedProduct

                                                      }
                                                  },
                                                  User=new UserViewModelForOrder
                                                  {
                                                      SenderId=x.UserId,
                                                      SenderEmail=x.User.Email,
                                                      SenderImageUrl=x.User.ProfileImage,
                                                      SenderHomeAddress=x.User.HomeAddress,
                                                      SenderRating= (x.User.RatedUsers.Count() > 0
                                                                  ? x.User.RatedUsers.Sum(r => r.RatingPoint) / (x.User.RatedUsers.Count()) : 0),
                                                      SenderUsername=x.User.UserName
                                                  }
                                              }).SingleAsync();
            return detail;
        }

        public async Task<List<Order>> GetOrderByPostId(Guid postId)
        {
            return await _dbContext.Orders.Where(x => x.PostId == postId).OrderByDescending(x => x.CreationDate).ToListAsync();
        }

        public async Task<List<Order>> GetOrderByUserIdAndPostId(Guid userId,Guid postId)
        {
            return await _dbContext.Orders.Where(x => x.UserId == userId&&x.PostId==postId).OrderByDescending(x => x.CreationDate).AsNoTracking().ToListAsync();
        }
        public async Task<List<ReceiveOrderViewModel>> GetAllOrderBy2UserId(Guid userId1, Guid userId2)
        {
            var listOrder = await _dbContext.Orders
                                             .Include(x => x.User).ThenInclude(u => u.VerifyUser).AsSplitQuery()
                                             .Include(x => x.User).ThenInclude(u => u.Raters).AsSplitQuery()
                                             .Include(x => x.Post).ThenInclude(p => p.Product).ThenInclude(p => p.Category).AsSplitQuery()
                                             .Include(x => x.Post).ThenInclude(p => p.Product).ThenInclude(p => p.ConditionType).AsSplitQuery()
                                             .Include(x => x.Status).AsSplitQuery()
                                             .Where(x => x.IsDelete == false &&
                                                ((x.Post.CreatedBy == userId1 && x.UserId == userId2) ||
                                                (x.Post.CreatedBy == userId2 && x.UserId == userId1)))
                                             .Select(x => new ReceiveOrderViewModel
                                             {
                                                 OrderId = x.Id,
                                                 OrderMessage = x.OrderMessage,
                                                 OrderStatus = x.Status.StatusName,
                                                 CreationDate = x.CreationDate,
                                                 Post = new PostViewModelForOrder
                                                 {
                                                     PostId = x.PostId,
                                                     PostContent = x.Post.PostContent,
                                                     PostTitle = x.Post.PostTitle,
                                                     Product = new ProductModel
                                                     {
                                                         CategoryId = x.Post.Product.CategoryId,
                                                         CategoryName = x.Post.Product.Category.CategoryName,
                                                         ConditionId = x.Post.Product.ConditionId,
                                                         ConditionName = x.Post.Product.ConditionType.ConditionType,
                                                         ProductId = x.Post.Product.Id,
                                                         ProductImageUrl = x.Post.Product.ProductImageUrl,
                                                         ProductPrice = x.Post.Product.ProductPrice,
                                                         ProductStatus = x.Post.Product.ProductStatus,
                                                         RequestedProduct = x.Post.Product.RequestedProduct
                                                     }

                                                 },
                                                 User = _dbContext.Users.Where(u => u.Id == x.CreatedBy).AsSplitQuery().Select(u => new UserViewModelForOrder
                                                 {
                                                     SenderId = x.CreatedBy.Value,
                                                     SenderEmail = u.Email,
                                                     SenderHomeAddress = u.HomeAddress,
                                                     SenderImageUrl = u.ProfileImage,
                                                     SenderRating = (u.RatedUsers.Count() > 0
                                                                  ? u.RatedUsers.Sum(r => r.RatingPoint) / (u.RatedUsers.Count()) : 0),
                                                     SenderUsername = u.UserName
                                                 }).Single()
                                             }).AsQueryable().AsNoTracking().OrderByDescending(x => x.CreationDate).ToListAsync();
            return listOrder;
        }

        public async Task<List<ReceiveOrderViewModel>> GetAllOrderByUserId(Guid userId)
        {
            var listOrder = await _dbContext.Orders
                                             .Include(x => x.User).ThenInclude(u => u.VerifyUser).AsSplitQuery()
                                             .Include(x => x.User).ThenInclude(u => u.Raters).AsSplitQuery()
                                             .Include(x => x.Post).ThenInclude(p => p.Product).ThenInclude(p => p.Category).AsSplitQuery()
                                             .Include(x => x.Post).ThenInclude(p => p.Product).ThenInclude(p => p.ConditionType).AsSplitQuery()
                                             .Include(x => x.Status).AsSplitQuery()
                                             .Where(x => x.IsDelete == false &&
                                                (x.Post.CreatedBy == userId  || x.UserId == userId))
                                             .Select(x => new ReceiveOrderViewModel
                                             {
                                                 OrderId = x.Id,
                                                 OrderMessage = x.OrderMessage,
                                                 OrderStatus = x.Status.StatusName,
                                                 CreationDate = x.CreationDate,
                                                 Post = new PostViewModelForOrder
                                                 {
                                                     PostId = x.PostId,
                                                     PostContent = x.Post.PostContent,
                                                     PostTitle = x.Post.PostTitle,
                                                     Product = new ProductModel
                                                     {
                                                         CategoryId = x.Post.Product.CategoryId,
                                                         CategoryName = x.Post.Product.Category.CategoryName,
                                                         ConditionId = x.Post.Product.ConditionId,
                                                         ConditionName = x.Post.Product.ConditionType.ConditionType,
                                                         ProductId = x.Post.Product.Id,
                                                         ProductImageUrl = x.Post.Product.ProductImageUrl,
                                                         ProductPrice = x.Post.Product.ProductPrice,
                                                         ProductStatus = x.Post.Product.ProductStatus,
                                                         RequestedProduct = x.Post.Product.RequestedProduct
                                                     }

                                                 },
                                                 User = _dbContext.Users.Where(u => u.Id == x.CreatedBy).AsSplitQuery().Select(u => new UserViewModelForOrder
                                                 {
                                                     SenderId = x.CreatedBy.Value,
                                                     SenderEmail = u.Email,
                                                     SenderHomeAddress = u.HomeAddress,
                                                     SenderImageUrl = u.ProfileImage,
                                                     SenderRating = (u.RatedUsers.Count() > 0
                                                                  ? u.RatedUsers.Sum(r => r.RatingPoint) / (u.RatedUsers.Count()) : 0),
                                                     SenderUsername = u.UserName
                                                 }).Single()
                                             }).AsQueryable().AsNoTracking().OrderByDescending(x => x.CreationDate).ToListAsync();
            return listOrder;
        }

        public async Task<List<ReceiveOrderViewModel>> GetAllReceiveOrderBy2UserId(Guid orderCreatedBy, Guid postOwnerId)
        {
            var listOrder = await _dbContext.Orders
                                             .Include(x => x.User).ThenInclude(u => u.VerifyUser).AsSplitQuery()
                                             .Include(x => x.User).ThenInclude(u => u.Raters).AsSplitQuery()
                                             .Include(x => x.Post).ThenInclude(p => p.Product).ThenInclude(p => p.Category).AsSplitQuery()
                                             .Include(x => x.Post).ThenInclude(p => p.Product).ThenInclude(p => p.ConditionType).AsSplitQuery()
                                             .Include(x => x.Status).AsSplitQuery()
                                             .Where(x => x.IsDelete == false &&
                                                x.Post.CreatedBy == postOwnerId && x.UserId == orderCreatedBy)
                                             .Select(x => new ReceiveOrderViewModel
                                             {
                                                 OrderId = x.Id,
                                                 OrderMessage = x.OrderMessage,
                                                 OrderStatus = x.Status.StatusName,
                                                 CreationDate = x.CreationDate,
                                                 Post = new PostViewModelForOrder
                                                 {
                                                     PostId = x.PostId,
                                                     PostContent = x.Post.PostContent,
                                                     PostTitle = x.Post.PostTitle,
                                                     Product = new ProductModel
                                                     {
                                                         CategoryId = x.Post.Product.CategoryId,
                                                         CategoryName = x.Post.Product.Category.CategoryName,
                                                         ConditionId = x.Post.Product.ConditionId,
                                                         ConditionName = x.Post.Product.ConditionType.ConditionType,
                                                         ProductId = x.Post.Product.Id,
                                                         ProductImageUrl = x.Post.Product.ProductImageUrl,
                                                         ProductPrice = x.Post.Product.ProductPrice,
                                                         ProductStatus = x.Post.Product.ProductStatus,
                                                         RequestedProduct = x.Post.Product.RequestedProduct
                                                     }

                                                 },
                                                 User = _dbContext.Users.Where(u => u.Id == x.CreatedBy).AsSplitQuery().Select(u => new UserViewModelForOrder
                                                 {
                                                     SenderId = x.CreatedBy.Value,
                                                     SenderEmail = u.Email,
                                                     SenderHomeAddress = u.HomeAddress,
                                                     SenderImageUrl = u.ProfileImage,
                                                     SenderRating = (u.RatedUsers.Count() > 0
                                                                  ? u.RatedUsers.Sum(r => r.RatingPoint) / (u.RatedUsers.Count()) : 0),
                                                     SenderUsername = u.UserName
                                                 }).Single()
                                             }).AsQueryable().AsNoTracking().OrderByDescending(x => x.CreationDate).ToListAsync();
            return listOrder;
        }

        public async Task<List<SentOrderViewModel>> GetAllSendOrderBy2UserId(Guid orderCreatedBy, Guid postOwnerId)
        {
            var listOrder = await _dbContext.Orders
                                             .Include(x => x.User).ThenInclude(u => u.VerifyUser).AsSplitQuery()
                                             .Include(x => x.User).ThenInclude(u => u.Raters).AsSplitQuery()
                                             .Include(x => x.Post).ThenInclude(p => p.Product).ThenInclude(p => p.Category).AsSplitQuery()
                                             .Include(x => x.Post).ThenInclude(p => p.Product).ThenInclude(p => p.ConditionType).AsSplitQuery()
                                             .Include(x => x.Status).AsSplitQuery()
                                             .Where(x => x.IsDelete == false &&
                                                x.Post.CreatedBy == postOwnerId && x.UserId == orderCreatedBy)
                                             .Select(x => new SentOrderViewModel
                                             {
                                                 OrderId = x.Id,
                                                 OrderMessage = x.OrderMessage,
                                                 OrderStatus = x.Status.StatusName,
                                                 CreationDate = x.CreationDate,
                                                 Post = new PostViewModelForOrder
                                                 {
                                                     PostId = x.PostId,
                                                     PostContent = x.Post.PostContent,
                                                     PostTitle = x.Post.PostTitle,
                                                     Product = new ProductModel
                                                     {
                                                         CategoryId = x.Post.Product.CategoryId,
                                                         CategoryName = x.Post.Product.Category.CategoryName,
                                                         ConditionId = x.Post.Product.ConditionId,
                                                         ConditionName = x.Post.Product.ConditionType.ConditionType,
                                                         ProductId = x.Post.Product.Id,
                                                         ProductImageUrl = x.Post.Product.ProductImageUrl,
                                                         ProductPrice = x.Post.Product.ProductPrice,
                                                         ProductStatus = x.Post.Product.ProductStatus,
                                                         RequestedProduct = x.Post.Product.RequestedProduct
                                                     }

                                                 },
                                                 User = _dbContext.Users.Where(u => u.Id == x.CreatedBy).AsSplitQuery().Select(u => new UserViewModelForOrder
                                                 {
                                                     SenderId = x.CreatedBy.Value,
                                                     SenderEmail = u.Email,
                                                     SenderHomeAddress = u.HomeAddress,
                                                     SenderImageUrl = u.ProfileImage,
                                                     SenderRating = (u.RatedUsers.Count() > 0
                                                                  ? u.RatedUsers.Sum(r => r.RatingPoint) / (u.RatedUsers.Count()) : 0),
                                                     SenderUsername = u.UserName
                                                 }).Single()
                                             }).AsQueryable().AsNoTracking().OrderByDescending(x => x.CreationDate).ToListAsync();
            return listOrder;
        }

        public async Task<List<OrderViewModelForWeb>> GetAllOrderForWeb()
        {
            var listOrder = await _dbContext.Orders.Where(x => x.IsDelete == false)
                                            .Include(x => x.User).ThenInclude(u => u.VerifyUser).AsSplitQuery()
                                            .Include(x => x.User).ThenInclude(u => u.Raters).AsSplitQuery()
                                            .Include(x => x.Post).AsSplitQuery()
                                            .Include(x => x.Status).AsSplitQuery()
                                            .Select(x => new OrderViewModelForWeb
                                            {
                                                Id = x.Id,
                                                OrderMessage = x.OrderMessage,
                                                OrderStatus = x.Status.StatusName,
                                                CreationDate = x.CreationDate,
                                                Post = new PostViewModelForOrder
                                                {
                                                    PostId = x.PostId,
                                                    PostContent = x.Post.PostContent,
                                                    PostTitle = x.Post.PostTitle
                                                },
                                                User = _dbContext.Users.Where(u => u.Id == x.UserId).AsSplitQuery().Select(u => new UserViewModelForOrder
                                                {
                                                    SenderId = x.CreatedBy.Value,
                                                    SenderEmail = u.Email,
                                                    SenderHomeAddress = u.HomeAddress,
                                                    SenderImageUrl = u.VerifyUser.UserImage,
                                                    SenderRating = (u.RatedUsers.Count() > 0
                                                                 ? u.RatedUsers.Sum(r => r.RatingPoint) / (u.RatedUsers.Count()) : 0),
                                                    SenderUsername = u.UserName
                                                }).Single()
                                            }).AsQueryable().AsNoTracking().OrderByDescending(x => x.CreationDate).ToListAsync();
            return listOrder;
        }
    }
}
