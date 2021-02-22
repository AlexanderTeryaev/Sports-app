using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using AppModel;

namespace DataService
{
    public class CommentsRepo : BaseRepo
    {
        public void Create(string comment, string fullName)
        {
            var item = new Comments
            {
                Comment = comment,
                FullName = fullName,
                AddDate = DateTime.Now
            };

            db.Comments.Add(item);
        }

        public IQueryable<Comments> GetQuery(bool archive)
        {
            return db.Comments.Where(t => t.IsArchive == archive);
        }

        public void RemoveList(int[] list)
        {
            foreach (var id in list)
            {
                var c = new Comments { CommentId = id };
                db.Comments.Attach(c);
                c.IsArchive = true;
            }
        }

        public void Remove(int id)
        {
            var c = db.Comments.Find(id);
            c.IsArchive = true;
        }
    }
}
