if not exists(select 1 from sys.tables where name = 'GridAreaOwnerBak')
begin
    select *
    into settlementreports.GridAreaOwnerBak
    from settlementreports.gridareaowner
end

update settlementreports.gridareaowner
set validfrom = '2020-01-01'
where id in (select g.id
             from settlementreports.gridareaowner g
             join (select code, min(validfrom) as validfrom
                   from settlementreports.gridareaowner
                   group by code) temp
             on g.code = temp.code and g.validfrom = temp.validfrom)
