using Dapper;
using System;
using System.Data;

namespace zennplay_launcher.Services
{
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }

        public override Guid Parse(object value)
        {
            if (value is string str && Guid.TryParse(str, out var guid))
            {
                return guid;
            }
            if (value is Guid g)
            {
                return g;
            }

            return Guid.Empty;
        }
    }
}