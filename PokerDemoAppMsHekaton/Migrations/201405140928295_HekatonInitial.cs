namespace PokerDemoAppMsHekaton.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HekatonInitial : DbMigration
    {
        public override void Up()
        {
            Sql("ALTER DATABASE [PlayersDemoDb] ADD FILEGROUP [MemFG] CONTAINS MEMORY_OPTIMIZED_DATA", true);
            Sql("ALTER DATABASE [PlayersDemoDb] ADD FILE ( NAME = N'MemData', FILENAME = N'C:\\Data\\MemData') TO FILEGROUP [MemFG]", true);

            Sql("ALTER DATABASE [PlayersDemoDb] SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT = ON WITH ROLLBACK IMMEDIATE", true);

            //TODO:bucket count - move to config
            Sql("CREATE TABLE [Players] ([PlayerID] [int] NOT NULL PRIMARY KEY NONCLUSTERED " +
                " HASH WITH (BUCKET_COUNT = 16384), [FullName] [nvarchar](70) NULL) " +
                " WITH (MEMORY_OPTIMIZED = ON)", true);

            Sql("CREATE TABLE [Accounts]([AccountId] [int] NOT NULL PRIMARY KEY NONCLUSTERED " +
                " HASH WITH (BUCKET_COUNT = 16384), [AccountType] [int] NOT NULL, " +
                " [Balance] [int] NOT NULL,[Player_PlayerId] [int] NULL) " +
                " WITH (MEMORY_OPTIMIZED = ON)", true);
        }
        
        public override void Down()
        {
            //DropForeignKey("dbo.Accounts", "Player_PlayerId", "dbo.Players");
            //DropIndex("dbo.Accounts", new[] { "Player_PlayerId" });
            DropTable("dbo.Players");
            DropTable("dbo.Accounts");
        }
    }
}
