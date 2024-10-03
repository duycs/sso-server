# SSO Identity

## Criteria
Create a Single sign on authentication server, support development many applications with only once sign on

## Development environment
- Visual Studio Code
- IdentityServer4
- MySQL

## Setup

#### To run the demo

**1.** Clone/Fork this repository.

**2.** Create the database on MySQL server by using the dotnet cli to run the migrations from within the command

#### Create EFMigrateHistory table at each database
```
CREATE TABLE `__EFMigrationsHistory` ( `MigrationId` nvarchar(150) NOT NULL, 
`ProductVersion` nvarchar(32) NOT NULL, 
PRIMARY KEY (`MigrationId`) );
```

### Set environment
```$env:ASPNETCORE_ENVIRONMENT = 'Development'```

### Migrate database

<pre><code>dotnet ef database update --context AppIdentityDbContext</code></pre>
<pre><code>dotnet ef database update --context PersistedGrantDbContext</code></pre>

#### Force delete all table if need
```
SET FOREIGN_KEY_CHECKS = 0; 
SET @tables = NULL;
SELECT GROUP_CONCAT(table_schema, '.', table_name) INTO @tables
  FROM information_schema.tables 
  WHERE table_schema = 'identity'; -- specify DB name here.
SET @tables = CONCAT('DROP TABLE ', @tables);
PREPARE stmt FROM @tables;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;ls
SET FOREIGN_KEY_CHECKS = 1; 
```

## References
- https://en.wikipedia.org/wiki/Single_sign-on
- https://github.com/mmacneil/AngularASPNETCoreOAuth
- https://github.com/echuck66/ASPNETMVCCoreTest
