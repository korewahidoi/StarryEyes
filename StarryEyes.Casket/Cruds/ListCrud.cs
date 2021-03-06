﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarryEyes.Casket.Cruds.Scaffolding;
using StarryEyes.Casket.DatabaseModels;

namespace StarryEyes.Casket.Cruds
{
    public class ListCrud : CrudBase<DatabaseList>
    {
        public ListCrud()
            : base(ResolutionMode.Replace)
        {
        }

        internal override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.CreateIndexAsync("ListFName", "FullName", false);
            await this.CreateIndexAsync("ListUID", "UserId", false);
            await this.CreateIndexAsync("ListSlug", "Slug", false);
        }

        public async Task RegisterListAsync(DatabaseList list)
        {
            await this.InsertAsync(list);
        }

        public async Task<DatabaseList> GetAsync(long userId, string slug)
        {
            return (await this.QueryAsync<DatabaseList>(
                CreateSql("UserId = @userId and LOWER(Slug) = LOWER(@slug) limit 1"),
                new { userId, slug })).FirstOrDefault();
        }

        public async Task<IEnumerable<DatabaseList>> FindOwnedListAsync(long userId)
        {
            return (await this.QueryAsync<DatabaseList>(
                CreateSql("UserId = @userId"),
                new { userId }));
        }
    }
}
