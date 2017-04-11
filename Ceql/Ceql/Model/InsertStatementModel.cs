﻿using Ceql.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ceql.Model
{
    public class InsertStatementModel<T> where T : ITable
    {


        public IEnumerable<PropertyInfo> Fields { get; set; }
        public string TableName { get; set; }
        public string SchemaName { get; set; }

        private IConnectorFormatter _formatter;

        public string Sql
        {
            get
            {
                return GetSql();
            }
        }


        public InsertStatementModel(IConnectorFormatter formatter)
        {
            _formatter = formatter;
        }

        public string ApplyParameters(T value)
        {
            var counter = 0;
            // todo (dr): inefficient, refactor
            var sql = Sql;
            foreach (var field in Fields)
            {
                var v = field.GetValue(value);
                sql = sql.Replace(("@p" + counter), _formatter.Format(v).ToString());
                counter++;
            }
            return sql;
        }

        private object lck = new object();
        private string _sql;
        private string GetSql()
        {

            if (_sql != null)
            {
                return _sql;
            }

            lock (lck)
            {
                if (_sql != null)
                {
                    return _sql;
                }


                var count = 0;
                _sql = String.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                    _formatter.TableNameEscape(SchemaName, TableName),
                    String.Join(",", Fields.Select(f => f.GetCustomAttribute<Contracts.Attributes.Field>().Name)),
                    String.Join(",", Fields.Select(f => "@p" + count++)));

                return _sql;
            }
        }
    }
}