

using System.Data;

namespace DataAccess
{
    /// <summary>
    /// Class DalParameter.
    /// </summary>
    public class DalParameter
    {
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        /// <value>The name of the parameter.</value>
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        /// <value>The type of the parameter.</value>
        public SqlDbType ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the parameter direction.
        /// </summary>
        /// <value>The parameter direction.</value>
        public ParameterDirection ParameterDirection { get; set; }

        /// <summary>
        /// Gets or sets the parameter value.
        /// </summary>
        /// <value>The parameter value.</value>
        public object ParameterValue { get; set; }

        /// <summary>
        /// Gets or sets the size of the parameter.
        /// </summary>
        /// <value>The size of the parameter.</value>
        public int ParameterSize { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DalParameter"/> class.
        /// </summary>
        public DalParameter()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DalParameter"/> class.
        /// </summary>
        /// <param name="parameterName">Name of the _ parameter.</param>
        /// <param name="parameterType">Type of the _ parameter.</param>
        /// <param name="parameterDirection">The _ parameter direction.</param>
        /// <param name="parameterValue">The _ parameter value.</param>
        public DalParameter(string parameterName, SqlDbType parameterType, ParameterDirection parameterDirection = ParameterDirection.Input, object parameterValue = null)
        {
            this.ParameterName = parameterName;
            this.ParameterType = parameterType;
            this.ParameterDirection = parameterDirection;
            this.ParameterValue = parameterValue;
        }
    }

    
}
