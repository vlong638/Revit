namespace MyRevit.SubsidenceMonitor.Entities
{
    public class ElementParameterValueSet
    {
        public ElementParameterValueSet(int elementId, string parameterName, string parameterValue)
        {
            ElementId = elementId;
            ParameterName = parameterName;
            ParameterValue = parameterValue;
        }

        public int ElementId { set; get; }
        public string ParameterName { set; get; }
        public string ParameterValue { set; get; }
    }
}
