﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Text;
using System.Text.RegularExpressions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.SettlementReport.Common.Infrastructure.Options;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Experimental;

public sealed class DatabricksSqlQueryBuilder
{
    private readonly DbContext _context;
    private readonly IOptions<DeltaTableOptions> _options;
    private readonly ILogger<DatabricksSqlQueryBuilder> _logger;

    public DatabricksSqlQueryBuilder(
        DbContext context,
        IOptions<DeltaTableOptions> options,
        ILoggerFactory loggerFactory)
    {
        _context = context;
        _options = options;
        _logger = loggerFactory.CreateLogger<DatabricksSqlQueryBuilder>();
    }

    public DatabricksStatement Build(DatabricksSqlQueryable query)
    {
        return Build(query, q => q);
    }

    public DatabricksStatement Build(DatabricksSqlQueryable query, Func<string, string> extendRawSql)
    {
        var sqlStatement = PrepareSqlStatement(query, extendRawSql, out var sqlParameters);

        var databricksStatement = DatabricksStatement.FromRawSql(sqlStatement);

        foreach (var (paramName, paramValue) in sqlParameters)
        {
            databricksStatement.WithParameter(paramName, paramValue);
        }

        return databricksStatement.Build();
    }

    public string BuildDebugString(DatabricksSqlQueryable query)
    {
        try
        {
            return PrepareSqlStatement(query, q => q, out _);
        }
        catch (NotSupportedException ex)
        {
            return $"Query does not support translation: {ex.Message}";
        }
    }

    private string PrepareSqlStatement(DatabricksSqlQueryable query, Func<string, string> extendRawQuery, out IEnumerable<KeyValuePair<string, string>> sqlParameters)
    {
        using var dbCommand = query.CreateDbCommand();

        var inputQuery = extendRawQuery(dbCommand.CommandText);

        var sqlStatement = new StringBuilder(inputQuery);
        var sqlParams = new List<KeyValuePair<string, string>>();

        var typeMapper = _context.GetService<IRelationalTypeMappingSource>();

        foreach (SqlParameter parameter in dbCommand.Parameters)
        {
            string parameterSubstitution;

            if (parameter.Value == null)
            {
                parameterSubstitution = "NULL";
            }
            else if (parameter.Value is string str)
            {
                var parameterName = parameter.ParameterName[1..];
                parameterSubstitution = $":{parameterName}";

                sqlParams.Add(new KeyValuePair<string, string>(parameterName, str));
            }
            else
            {
                parameterSubstitution = typeMapper.GetMapping(parameter.Value.GetType()).GenerateSqlLiteral(parameter.Value);
            }

            sqlStatement = sqlStatement.Replace(parameter.ParameterName, parameterSubstitution);
        }

        sqlParameters = sqlParams;
        var translated = TranslateTransactToAnsi(sqlStatement);
        var logString = $"Translated SQL: {translated}" + Environment.NewLine + string.Join(Environment.NewLine, sqlParams.Select(x => $"Parameter Name: {x.Key} | Value: {x.Value}"));
        _logger.LogInformation(logString);
        return translated;
    }

    private string TranslateTransactToAnsi(StringBuilder transactSqlQuery)
    {
        var strBuilder = transactSqlQuery
            .Replace('[', '`')
            .Replace(']', '`')
            .Replace('"', '\'')
            .Replace('"', '\'')
            .Replace($"`{_options.Value.DatabricksCatalogName}.", $"`{_options.Value.DatabricksCatalogName}`.`");

        var ansiSql = strBuilder.ToString();

        ansiSql = Regex.Replace(ansiSql, "N'([^']+)'", "'$1'");
        ansiSql = Regex.Replace(ansiSql, "OFFSET ([^\\s]+) ROWS", "OFFSET $1");
        ansiSql = Regex.Replace(ansiSql, "FETCH NEXT ([^\\s]+) ROWS ONLY", "LIMIT $1");
        ansiSql = Regex.Replace(ansiSql, "OFFSET ([^\\s]+) LIMIT ([^\\s]+)", "LIMIT $2 OFFSET $1");

        return ansiSql;
    }
}
