﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using StarryEyes.Anomaly.TwitterApi.DataModels;
using StarryEyes.Anomaly.TwitterApi.Rest;
using StarryEyes.Anomaly.Utils;
using StarryEyes.Models.Accounting;
using StarryEyes.Settings;

namespace StarryEyes.Filters.Sources
{
    public class FilterUser : FilterSourceBase
    {
        private readonly string _targetIdOrScreenName;

        public FilterUser() { }

        public FilterUser(string screenName)
        {
            _targetIdOrScreenName = screenName.Trim();
            if (!screenName.StartsWith("#")) return;
            // if specified by ID, check variable is numerics
            long result;
            if (!Int64.TryParse(screenName.Substring(1), out result))
            {
                throw new ArgumentException("不正な引数が指定されています(ソースフィルタ user)");
            }
        }

        public override string FilterKey
        {
            get { return "user"; }
        }

        public override string FilterValue
        {
            get { return _targetIdOrScreenName; }
        }

        public override Func<TwitterStatus, bool> GetEvaluator()
        {
            return this._targetIdOrScreenName.StartsWith("#")
                ? (Func<TwitterStatus, bool>)(s => s.User.Id == Int64.Parse(this._targetIdOrScreenName.Substring(1)))
                : (s => s.User.ScreenName.Equals(this._targetIdOrScreenName, StringComparison.CurrentCultureIgnoreCase));
        }

        public override string GetSqlQuery()
        {
            if (_targetIdOrScreenName.StartsWith("#"))
            {
                // extract by id
                return "BaseUserId = " + _targetIdOrScreenName.Substring(1);
            }
            // extract by screen name
            return "EXISTS (select ScreenName from User where Id = status.BaseUserId AND " +
                   "LOWER(ScreenName) = '" + this._targetIdOrScreenName.ToLower() + "' limit 1)";
        }

        protected override IObservable<TwitterStatus> ReceiveSink(long? maxId)
        {
            Func<TwitterAccount, Task<IEnumerable<TwitterStatus>>> uif;
            if (_targetIdOrScreenName.StartsWith("#"))
            {
                uif = a => a.GetUserTimelineAsync(Int64.Parse(_targetIdOrScreenName.Substring(1)),
                    includeRetweets: true);
            }
            else
            {
                uif = a => a.GetUserTimelineAsync(_targetIdOrScreenName,
                    includeRetweets: true);
            }
            return Observable.Start(() => Setting.Accounts.GetRandomOne())
                             .Where(a => a != null)
                             .SelectMany(a => uif(a).ToObservable());
        }
    }
}
