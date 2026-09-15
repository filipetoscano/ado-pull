Queries
===============================================================================

Below, some sample queries to run on the Sqlite database extract.


Iteration vs State
-------------------------------------------------------------------------------

Returns the nunber of issues per state, grouped by iteration. Expectation is
that:

- Past iterations should be in done state
- Current iteration should have issues in progress
- Future iterations should be in planning, or in `Needs Information`

```
select
    coalesce(i.Name, '(No iteration)')                          as Iteration,
    sum(case when w.State = 'To Do'             then 1 else 0 end) as [To Do],
    sum(case when w.State = 'Needs Information' then 1 else 0 end) as [Needs Information],
    sum(case when w.State = 'Doing'             then 1 else 0 end) as [Doing],
    sum(case when w.State = 'Fixed on Dev'      then 1 else 0 end) as [Fixed on Dev],
    sum(case when w.State = 'Reopened'          then 1 else 0 end) as [Reopened],
    sum(case when w.State = 'Ready on UAT'      then 1 else 0 end) as [Ready on UAT],
    sum(case when w.State = 'On Hold'           then 1 else 0 end) as [On Hold],
    sum(case when w.State = 'Deferred'          then 1 else 0 end) as [Deferred],
    sum(case when w.State = 'Done'              then 1 else 0 end) as [Done],
    sum(case when w.State = 'Cancelled'         then 1 else 0 end) as [Cancelled],
    count(*)                                                    as Total
from WorkItems w
left join Iterations i on ( i.Id = w.IterationId )
where w.Component = 'Infrastructure'
group by i.Id, i.Name
order by i.DateStart is null, i.DateStart, i.Name;
```


Re-opened stats
-------------------------------------------------------------------------------

By component, how many items were created, had a re-open, and the number of
re-opens.

```
select
    coalesce(w.Component, '(No component)')  as Component,
    count(distinct w.Id)                     as [# of Items],
    count(distinct t.ItemId)                 as [# of Items Re-opened],
    count(t.ItemId)                          as [# Re-opens]
from WorkItems w
left join WorkItemTransitions t on ( t.ItemId = w.Id and t.[To] = 'Reopened' )
where w.State = 'Done'
group by w.Component
order by [# Re-opens] desc, Component;
```


Re-opened (list of items)
-------------------------------------------------------------------------------

Which items were re-opened, and how many times.

```
select
    w.Id            as [Issue #],
    w.Component     as Component,
    w.IssueType     as [Issue Type],
    count(*)        as [Times Reopened]
from WorkItems w
join WorkItemTransitions t on ( t.ItemId = w.Id and t.[To] = 'Reopened' )
-- where w.State = 'Done'
group by w.Id, w.Component, w.IssueType
order by [Times Reopened] desc, w.Id;
```
