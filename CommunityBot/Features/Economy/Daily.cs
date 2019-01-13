﻿using System;
using System.Collections.Generic;
using CommunityBot.Features.GlobalAccounts;

namespace CommunityBot.Features.Economy
{
    public class Daily : IDailyMiunies
    {
        private readonly IGlobalUserAccounts globalUserAccountProvider;

        public Daily(IGlobalUserAccounts globalUserAccountProvider)
        {
            this.globalUserAccountProvider = globalUserAccountProvider;
        }
        
        public void GetDaily(ulong userId)
        {
            var account = globalUserAccountProvider.GetUserAccount(userId);
            var sinceLastDaily = DateTime.UtcNow - account.LastDaily;

            if (sinceLastDaily.TotalHours < 24)
            {
                var e = new InvalidOperationException(Constants.ExDailyTooSoon);
                e.Data.Add("sinceLastDaily", sinceLastDaily);
                throw e;
            }

            account.Miunies += Constants.DailyMuiniesGain;
            account.LastDaily = DateTime.UtcNow;

            globalUserAccountProvider.SaveAccounts(userId);
        }
    }
}
