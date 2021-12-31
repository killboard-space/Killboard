﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Killboard.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Killboard.Data.Models
{
    public partial class KillboardContext
    {
        private KillboardContextProcedures _procedures;

        public virtual KillboardContextProcedures Procedures
        {
            get
            {
                if (_procedures is null) _procedures = new KillboardContextProcedures(this);
                return _procedures;
            }
            set
            {
                _procedures = value;
            }
        }

        public KillboardContextProcedures GetProcedures()
        {
            return Procedures;
        }
    }

    public partial class KillboardContextProcedures
    {
        private readonly KillboardContext _context;

        public KillboardContextProcedures(KillboardContext context)
        {
            _context = context;
        }

        public virtual async Task<List<procGetKillmailDetailsResult>> procGetKillmailDetailsAsync(int? killmailId, int? allianceId, int? corporationId, int? characterId, int? attackerId, int? systemId, int? constellationId, int? regionId, int? shipTypeId, int? groupId, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default)
        {
            var parameterreturnValue = new SqlParameter
            {
                ParameterName = "returnValue",
                Direction = System.Data.ParameterDirection.Output,
                SqlDbType = System.Data.SqlDbType.Int,
            };

            var sqlParameters = new []
            {
                new SqlParameter
                {
                    ParameterName = "killmailId",
                    Value = killmailId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "allianceId",
                    Value = allianceId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "corporationId",
                    Value = corporationId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "characterId",
                    Value = characterId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "attackerId",
                    Value = attackerId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "systemId",
                    Value = systemId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "constellationId",
                    Value = constellationId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "regionId",
                    Value = regionId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "shipTypeId",
                    Value = shipTypeId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "groupId",
                    Value = groupId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                parameterreturnValue,
            };
            var _ = await _context.SqlQueryAsync<procGetKillmailDetailsResult>("EXEC @returnValue = [dbo].[procGetKillmailDetails] @killmailId, @allianceId, @corporationId, @characterId, @attackerId, @systemId, @constellationId, @regionId, @shipTypeId, @groupId", sqlParameters, cancellationToken);

            returnValue?.SetValue(parameterreturnValue.Value);

            return _;
        }

        public virtual async Task<List<procGetSystemsInJumpRangeResult>> procGetSystemsInJumpRangeAsync(int? fromSystemId, int? shipId, int? jdcLevel, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default)
        {
            //var parameterreturnValue = new SqlParameter
            //{
            //    ParameterName = "returnValue",
            //    Direction = System.Data.ParameterDirection.Output,
            //    SqlDbType = System.Data.SqlDbType.Int,
            //};

            var sqlParameters = new []
            {
                new SqlParameter
                {
                    ParameterName = "fromSystemId",
                    Value = fromSystemId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "shipId",
                    Value = shipId ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                {
                    ParameterName = "jdcLevel",
                    Value = jdcLevel ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                //parameterreturnValue,
            };
            //var _ = await _context.SqlQueryAsync<procGetSystemsInJumpRangeResult>("EXEC @returnValue = [dbo].[procGetSystemsInJumpRange] @fromSystemId, @shipId, @jdcLevel", sqlParameters, cancellationToken);
            var _ = await _context.SqlQueryAsync<procGetSystemsInJumpRangeResult>("EXEC [dbo].[procGetSystemsInJumpRange] @fromSystemId, @shipId, @jdcLevel", sqlParameters, cancellationToken);

            //returnValue?.SetValue(parameterreturnValue.Value);

            return _;
        }
    }
}
