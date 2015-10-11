using AccountAtAGlance.Repository.Helpers;
using Microsoft.Data.Entity;
using System;
using Microsoft.Framework.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.Data.Entity.SqlServer;
using AccountAtAGlance.Repository.Interfaces;

namespace AccountAtAGlance.Repository.Seeding
{
    public static class DatabaseInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using (var context = serviceProvider.GetService<AccountAtAGlanceContext>())
            {
                var db = context.Database;
                if (db != null)
                {
                    if (await db.EnsureCreatedAsync())
                    {
                        await InsertSampleData(serviceProvider);
                    }
                }
                else
                {
                    await InsertSampleData(serviceProvider);
                }
            }
        }

        public static async Task InsertSampleData(IServiceProvider serviceProvider)
        {
            await Task.Run(async () =>
            {
                using (var context = serviceProvider.GetService<AccountAtAGlanceContext>())
                {
                    context.Database.ExecuteSqlCommand(@"
                    CREATE PROCEDURE dbo.DeleteSecuritiesAndExchanges

                    AS
	                    BEGIN
	 
	 		                    BEGIN TRANSACTION
		                    BEGIN TRY
			                    DELETE FROM Position;   
			                    DELETE FROM Stock;
			                    DELETE FROM MutualFund;
			                    DELETE FROM Exchange; 
			                    DELETE FROM MarketIndex;
			                    COMMIT TRANSACTION
			                    SELECT 0				
		                    END TRY
		                    BEGIN CATCH
			                    ROLLBACK TRANSACTION
			                    SELECT -1		
		                    END CATCH
	
	                    END
                    ");

                    context.Database.ExecuteSqlCommand(@"
                    CREATE PROCEDURE dbo.DeleteAccounts

                    AS
	                    BEGIN

		                    BEGIN TRANSACTION
			                    BEGIN TRY
				                    DELETE FROM [Order];                                              
				                    DELETE FROM BrokerageAccount;
				                    DELETE FROM Customer;					
				                    COMMIT TRANSACTION
				                    SELECT 0				
			                    END TRY
			                    BEGIN CATCH
				                    ROLLBACK TRANSACTION
				                    SELECT -1		
			                    END CATCH
	                    END	
	                ");

                    var sr = serviceProvider.GetService<ISecurityRepository>();
                    await sr.InsertSecurityDataAsync();

                    var mr = serviceProvider.GetService<IMarketsAndNewsRepository>();
                    await mr.InsertMarketDataAsync();

                    var ar = serviceProvider.GetService<IAccountRepository>();
                    await ar.CreateCustomerAsync();
                    await ar.CreateAccountPositionsAsync();
                }
            });
        }
    }
}
