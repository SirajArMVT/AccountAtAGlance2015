using Microsoft.Data.Entity;
using System;
using System.Threading.Tasks;
using AccountAtAGlance.Repository.Interfaces;

namespace AccountAtAGlance.Repository.Seeding
{
    public class DatabaseInitializer
    {
        IAccountRepository _AccountRepository;
        ISecurityRepository _SecurityRepository;
        IMarketsAndNewsRepository _MarketsAndNewsRepository;
        AccountAtAGlanceContext _Context;

        public DatabaseInitializer(AccountAtAGlanceContext context, IAccountRepository acctRepo,
            ISecurityRepository securityRepo, IMarketsAndNewsRepository marketsRepo)
        {
            _Context = context;
            _AccountRepository = acctRepo;
            _SecurityRepository = securityRepo;
            _MarketsAndNewsRepository = marketsRepo;
        }

        public async Task SeedAsync()
        {
            var db = _Context.Database;
            if (db != null)
            {
                if (await db.EnsureCreatedAsync())
                {
                    await InsertSampleData();
                }
            }
            else
            {
                await InsertSampleData();
            }
        }

        public async Task InsertSampleData()
        {
            await Task.Run(async () =>
            {

                _Context.Database.ExecuteSqlCommand(@"
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

                _Context.Database.ExecuteSqlCommand(@"
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

                await _SecurityRepository.InsertSecurityDataAsync();
                await _MarketsAndNewsRepository.InsertMarketDataAsync();
                await _AccountRepository.CreateCustomerAsync();
                await _AccountRepository.CreateAccountPositionsAsync();
            });
        }
    }
}
