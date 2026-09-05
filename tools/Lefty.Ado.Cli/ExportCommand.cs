using Lefty.Ado.Model;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Lefty.Ado.Cli;

/// <summary />
[Command( "export", Description = "Export all work items into Sqlite database" )]
public class ExportCommand
{
    private readonly IAdoService _ado;


    /// <summary />
    public ExportCommand( IAdoService ado )
    {
        _ado = ado;
    }


    /// <summary />
    [Argument( 0, Description = "Project name" )]
    [Required]
    public string? Project { get; set; }

    /// <summary />
    [Option( "-o|--output-file", CommandOptionType.SingleValue, Description = "Output filename" )]
    public string? OutputFilename { get; set; }


    /// <summary />
    public async Task<int> OnExecuteAsync( CommandLineApplication app, CancellationToken cancellationToken )
    {
        /*
         * Fetch all work items from project
         */
        var items = await _ado.WorkItemListAsync( this.Project!, cancellationToken );


        /*
         * If not specified, infer the filename based on the current date time
         */
        if ( this.OutputFilename == null )
        {
            var now = DateTime.Now;
            this.OutputFilename = $"{this.Project}-{now:yyyyMMdd-HHmm}.db";
        }


        /*
         *
         */
        using var connection = new SqliteConnection( new SqliteConnectionStringBuilder { DataSource = this.OutputFilename }.ToString() );
        connection.Open();

        ExecuteResourceScript( connection, "SqliteSchema.sql" );
        ExecuteResourceScript( connection, "DatabaseReset.sql" );


        using var transaction = connection.BeginTransaction();

        var iterations = items
            .Select( i => i.Iteration )
            .Where( i => i is not null )
            .Select( i => i! )
            .GroupBy( i => i.Id )
            .Select( g => g.First() );

        var users = items
            .SelectMany( i => new[] { i.CreatedBy, i.AssignedTo }
                .Concat( i.Transitions.Select( t => t.By ) )
                .Concat( i.Remarks.Select( r => r.By ) ) )
            .Where( u => u is not null )
            .Select( u => u! )
            .GroupBy( u => u.Id )
            .Select( g => g.First() );

        try
        {
            InsertIterations( connection, transaction, iterations );
            InsertUsers( connection, transaction, users );
            InsertWorkItems( connection, transaction, items );
            InsertWorkItemRemarks( connection, transaction, items );
            InsertWorkItemTransitions( connection, transaction, items );

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }


        AnsiConsole.MarkupLineInterpolated( $"[green]ok[/]: exported {items.Count} work items to {this.OutputFilename}" );

        return 0;
    }


    /// <summary />
    private static void ExecuteResourceScript( SqliteConnection connection, string fileName )
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().Single( n => n.EndsWith( fileName, StringComparison.Ordinal ) );

        using var stream = assembly.GetManifestResourceStream( resourceName )!;
        using var reader = new StreamReader( stream );

        using var cmd = connection.CreateCommand();
        cmd.CommandText = reader.ReadToEnd();
        cmd.ExecuteNonQuery();
    }


    /// <summary />
    private static void InsertIterations( SqliteConnection connection, SqliteTransaction transaction, IEnumerable<Iteration> iterations )
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "insert into Iterations (Id, Name, DateStart, DateEnd) values ($Id, $Name, $DateStart, $DateEnd)";

        var id = cmd.CreateParameter(); id.ParameterName = "$Id"; cmd.Parameters.Add( id );
        var name = cmd.CreateParameter(); name.ParameterName = "$Name"; cmd.Parameters.Add( name );
        var dateStart = cmd.CreateParameter(); dateStart.ParameterName = "$DateStart"; cmd.Parameters.Add( dateStart );
        var dateEnd = cmd.CreateParameter(); dateEnd.ParameterName = "$DateEnd"; cmd.Parameters.Add( dateEnd );

        foreach ( var iteration in iterations )
        {
            id.Value = iteration.Id.ToString();
            name.Value = iteration.Name;
            dateStart.Value = iteration.DateStart is { } s ? s.ToString( "yyyy-MM-dd" ) : DBNull.Value;
            dateEnd.Value = iteration.DateEnd is { } e ? e.ToString( "yyyy-MM-dd" ) : DBNull.Value;

            cmd.ExecuteNonQuery();
        }
    }


    /// <summary />
    private static void InsertUsers( SqliteConnection connection, SqliteTransaction transaction, IEnumerable<User> users )
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "insert into AppUsers (Id, DisplayName, Upn) values ($Id, $DisplayName, $Upn)";

        var id = cmd.CreateParameter(); id.ParameterName = "$Id"; cmd.Parameters.Add( id );
        var displayName = cmd.CreateParameter(); displayName.ParameterName = "$DisplayName"; cmd.Parameters.Add( displayName );
        var upn = cmd.CreateParameter(); upn.ParameterName = "$Upn"; cmd.Parameters.Add( upn );

        foreach ( var user in users )
        {
            id.Value = user.Id.ToString();
            displayName.Value = user.DisplayName;
            upn.Value = user.Upn;

            cmd.ExecuteNonQuery();
        }
    }


    /// <summary />
    private static void InsertWorkItems( SqliteConnection connection, SqliteTransaction transaction, IReadOnlyList<WorkItem> items )
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = """
            insert into WorkItems
                (Id, Title, Description, State, CreatedByUserId, MomentCreated, MomentActivity, AssignedToUserId, Tags, IterationId, IssueType, Component, Severity)
            values
                ($Id, $Title, $Description, $State, $CreatedByUserId, $MomentCreated, $MomentActivity, $AssignedToUserId, $Tags, $IterationId, $IssueType, $Component, $Severity)
            """;

        var id = cmd.CreateParameter(); id.ParameterName = "$Id"; cmd.Parameters.Add( id );
        var title = cmd.CreateParameter(); title.ParameterName = "$Title"; cmd.Parameters.Add( title );
        var description = cmd.CreateParameter(); description.ParameterName = "$Description"; cmd.Parameters.Add( description );
        var state = cmd.CreateParameter(); state.ParameterName = "$State"; cmd.Parameters.Add( state );
        var createdByUserId = cmd.CreateParameter(); createdByUserId.ParameterName = "$CreatedByUserId"; cmd.Parameters.Add( createdByUserId );
        var momentCreated = cmd.CreateParameter(); momentCreated.ParameterName = "$MomentCreated"; cmd.Parameters.Add( momentCreated );
        var momentActivity = cmd.CreateParameter(); momentActivity.ParameterName = "$MomentActivity"; cmd.Parameters.Add( momentActivity );
        var assignedToUserId = cmd.CreateParameter(); assignedToUserId.ParameterName = "$AssignedToUserId"; cmd.Parameters.Add( assignedToUserId );
        var tags = cmd.CreateParameter(); tags.ParameterName = "$Tags"; cmd.Parameters.Add( tags );
        var iterationId = cmd.CreateParameter(); iterationId.ParameterName = "$IterationId"; cmd.Parameters.Add( iterationId );
        var issueType = cmd.CreateParameter(); issueType.ParameterName = "$IssueType"; cmd.Parameters.Add( issueType );
        var component = cmd.CreateParameter(); component.ParameterName = "$Component"; cmd.Parameters.Add( component );
        var severity = cmd.CreateParameter(); severity.ParameterName = "$Severity"; cmd.Parameters.Add( severity );

        foreach ( var item in items )
        {
            id.Value = item.Id;
            title.Value = item.Title;
            description.Value = item.Description;
            state.Value = item.State;
            createdByUserId.Value = item.CreatedBy.Id.ToString();
            momentCreated.Value = item.MomentCreated;
            momentActivity.Value = item.MomentActivity;
            assignedToUserId.Value = item.AssignedTo is { } a ? a.Id.ToString() : DBNull.Value;
            tags.Value = string.Join( ';', item.Tags );
            iterationId.Value = item.Iteration is { } it ? it.Id.ToString() : DBNull.Value;
            issueType.Value = (object?) item.IssueType ?? DBNull.Value;
            component.Value = (object?) item.Component ?? DBNull.Value;
            severity.Value = (object?) item.Severity ?? DBNull.Value;

            cmd.ExecuteNonQuery();
        }
    }


    /// <summary />
    private static void InsertWorkItemRemarks( SqliteConnection connection, SqliteTransaction transaction, IReadOnlyList<WorkItem> items )
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "insert into WorkItemRemarks (ItemId, Text, ByUserId, Moment) values ($ItemId, $Text, $ByUserId, $Moment)";

        var itemId = cmd.CreateParameter(); itemId.ParameterName = "$ItemId"; cmd.Parameters.Add( itemId );
        var text = cmd.CreateParameter(); text.ParameterName = "$Text"; cmd.Parameters.Add( text );
        var byUserId = cmd.CreateParameter(); byUserId.ParameterName = "$ByUserId"; cmd.Parameters.Add( byUserId );
        var moment = cmd.CreateParameter(); moment.ParameterName = "$Moment"; cmd.Parameters.Add( moment );

        foreach ( var item in items )
        {
            foreach ( var remark in item.Remarks )
            {
                itemId.Value = item.Id;
                text.Value = remark.Text;
                byUserId.Value = remark.By.Id.ToString();
                moment.Value = remark.Moment;

                cmd.ExecuteNonQuery();
            }
        }
    }


    /// <summary />
    private static void InsertWorkItemTransitions( SqliteConnection connection, SqliteTransaction transaction, IReadOnlyList<WorkItem> items )
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "insert into WorkItemTransitions (ItemId, [From], [To], ByUserId, Moment) values ($ItemId, $From, $To, $ByUserId, $Moment)";

        var itemId = cmd.CreateParameter(); itemId.ParameterName = "$ItemId"; cmd.Parameters.Add( itemId );
        var from = cmd.CreateParameter(); from.ParameterName = "$From"; cmd.Parameters.Add( from );
        var to = cmd.CreateParameter(); to.ParameterName = "$To"; cmd.Parameters.Add( to );
        var byUserId = cmd.CreateParameter(); byUserId.ParameterName = "$ByUserId"; cmd.Parameters.Add( byUserId );
        var moment = cmd.CreateParameter(); moment.ParameterName = "$Moment"; cmd.Parameters.Add( moment );

        foreach ( var item in items )
        {
            foreach ( var transitionEntry in item.Transitions )
            {
                itemId.Value = item.Id;
                from.Value = transitionEntry.From;
                to.Value = transitionEntry.To;
                byUserId.Value = transitionEntry.By.Id.ToString();
                moment.Value = transitionEntry.Moment;

                cmd.ExecuteNonQuery();
            }
        }
    }
}