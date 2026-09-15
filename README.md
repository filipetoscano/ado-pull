ado-pull
==========================================================================

Usage
--------------------------------------------------------------------------

```bash
export ADO_ORG=OrganizationName
export ADO_PAT=pat-with-read-permissions

./adopull
```

```
Retrieves/Queries items from Azure DevOps

Usage: adopull [command] [options]

Options:
  --version     Show version information.
  -?|-h|--help  Show help information.

Commands:
  export        Export all work items into Sqlite database
  json          Export all work items as JSON file

Run 'adopull [command] -?|-h|--help' for more information about a command
```


Schema
--------------------------------------------------------------------------



```
CREATE TABLE Iterations
(
    Id text not null primary key,     -- Guid
    Name text not null,               -- Name of iteration
    DateStart date null,              -- Start date (when available)
    DateEnd date null                 -- End date (when available)
);

CREATE TABLE AppUsers
(
    Id text not null primary key,     -- Guid
    DisplayName text not null,        -- Display name (as per Entra)
    Upn text not null                 -- Entra (UPN) user principal name
);

CREATE TABLE WorkItems
(
    Id integer not null primary key,                         -- Id
    Title text not null,                                     -- Item title
    Description text not null,                               -- Item description (in Markdown or HTML)
    State text not null,                                     -- State
    CreatedByUserId text not null references AppUsers (Id),  -- Created by
    MomentCreated datetime not null,                         -- Moment when item was created
    MomentActivity datetime not null,                        -- Moment when item was last updated
    AssignedToUserId text null references AppUsers (Id),     -- Assigned to
    Tags text not null,                                      -- List of tags, semi-colon separated
    IterationId text null references Iterations (Id),        -- Iteration
    IssueType text null,                                     -- Type of issue
    Component text null,                                     -- Component / Area
    Severity text null                                       -- Severity
);

CREATE TABLE WorkItemRemarks
(
    ItemId integer not null references WorkItems (Id),       -- Item identifier
    Text text not null,                                      -- Remarks text
    ByUserId text not null references AppUsers (Id),         -- User who wrote remark
    Moment datetime not null                                 -- Moment when remark was added
);

CREATE TABLE WorkItemTransitions
(
    ItemId integer not null references WorkItems (Id),       -- Item identifier
    [From] text not null,                                    -- From state
    [To] text not null,                                      -- To state
    ByUserId text not null references AppUsers (Id),         -- User who made change
    Moment datetime not null                                 -- Moment when transition was made
);
```


Howto: Generate a (PAT) Personal access token
--------------------------------------------------------------------------

- Go to your Azure DevOps Organization
- In top-right corner (next to your avatar), click on `User settings` > `Personal access tokens`
- Click `+ New Token` button and enter:
    - Name: `adopull`
    - Expiration: 90d
    - Scopes: (x) Custom defined
    - Build: [x] Read
    - Project and Team: [x] Read
    - Release: [x] Read
    - Work Items: [x] Read
- Click `Create`
- Copy and securely store your PAT
