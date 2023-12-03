using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");

        var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

        var context = new MyDbContext(loggerFactory);
        context.Database.EnsureCreated();
        InitializeData(context);

        Console.WriteLine("All posts:");
        var data = context.BlogPosts.Select(x => x.Title).ToList();
        Console.WriteLine(JsonSerializer.Serialize(data));
            
        
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Ivan: 4
        // Petr: 2
        // Elena: 3    
        Console.WriteLine("How many comments each user left:");
        var commentsCount = context.BlogComments
            .GroupBy(x => x.UserName)
            .Select(x => new { UserName = x.Key, CommentsCount = x.Count() });
        foreach (var comment in commentsCount)
        {
            Console.WriteLine($"{comment.UserName}: {comment.CommentsCount}");
        }

        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Post2: '2020-03-06', '4'
        // Post1: '2020-03-05', '8'
        // Post3: '2020-02-14', '9'
        Console.WriteLine("Posts ordered by date of last comment. Result should include text of last comment:");
        var postsOrderedByLastCommentDate = context.BlogPosts
            .GroupBy(x => x.Title)
            .ToDictionary(x => x.Key, x => x.SelectMany(p => p.Comments).OrderByDescending(c => c.CreatedDate).First())
            .Select(x => new
             {
                 Title = x.Key,
                 Date = x.Value.CreatedDate,
                 Comment = x.Value.Text,
             });
        foreach (var post in postsOrderedByLastCommentDate)
        {
            Console.WriteLine($"{post.Title}: {post.Date}, {post.Comment}");
        }


        // 'last comment' is the latest Comment in each Post
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Ivan: 2
        // Petr: 1
        Console.WriteLine("How many last comments each user left:");
        var numberOfLastCommentsLeftByUser = context.BlogComments
            .GroupBy(
                x => x.BlogPostId, 
                (_, comments) => new { comments.OrderByDescending(x => x.CreatedDate).First().UserName })
            .GroupBy(x => x.UserName)
            .Select(x => new
            {
                UserName = x.Key,
                Count = x.Count(),
            });
        foreach (var usersComments in numberOfLastCommentsLeftByUser)
        {
            Console.WriteLine($"{usersComments.UserName}: {usersComments.Count}");
        }



        // Console.WriteLine(
        //     JsonSerializer.Serialize(BlogService.NumberOfCommentsPerUser(context)));
        // Console.WriteLine(
        //     JsonSerializer.Serialize(BlogService.PostsOrderedByLastCommentDate(context)));
        // Console.WriteLine(
        //     JsonSerializer.Serialize(BlogService.NumberOfLastCommentsLeftByUser(context)));

    }

    private static void InitializeData(MyDbContext context)
    {
        context.BlogPosts.Add(new BlogPost("Post1")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("1", new DateTime(2020, 3, 2), "Petr"),
                new BlogComment("2", new DateTime(2020, 3, 4), "Elena"),
                new BlogComment("8", new DateTime(2020, 3, 5), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post2")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("3", new DateTime(2020, 3, 5), "Elena"),
                new BlogComment("4", new DateTime(2020, 3, 6), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post3")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("5", new DateTime(2020, 2, 7), "Ivan"),
                new BlogComment("6", new DateTime(2020, 2, 9), "Elena"),
                new BlogComment("7", new DateTime(2020, 2, 10), "Ivan"),
                new BlogComment("9", new DateTime(2020, 2, 14), "Petr"),
            }
        });
        context.SaveChanges();
    }
}