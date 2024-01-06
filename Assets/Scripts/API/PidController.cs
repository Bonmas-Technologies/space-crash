namespace AI.API
{
    public sealed class PidController
    {

        public PidController(double p, double i, double d)
        {
            D = d;
            I = i;
            P = p;
        }

        public double P { get; set; } = 0;
        public double I { get; set; } = 0;
        public double D { get; set; } = 0;

        public double SetPoint { get; set; } = 0;

        public double IntegralTerm { get; private set; } = 0;

        public double ProcessVariable
        {
            get { return processVariable; }
            set
            {
                ProcessVariableLast = processVariable;
                processVariable = value;
            }
        }

        public double ProcessVariableLast { get; private set; } = 0;


        private double processVariable = 0;

        public double ControlVariable(float deltaTime)
        {
            double error = SetPoint - ProcessVariable;

            double proportionalTerm = P * error;
            IntegralTerm += (I * error * deltaTime);
            double derivativeTerm = D * ((processVariable - ProcessVariableLast) / deltaTime);

            return proportionalTerm + IntegralTerm - derivativeTerm;
        }
    }
}
