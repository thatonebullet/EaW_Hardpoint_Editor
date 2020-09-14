using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardpointEditor
{
    abstract class Singleton<DerivedType>
    {
        private static DerivedType m_instance;

        public static DerivedType Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = (DerivedType)Activator.CreateInstance(typeof(DerivedType), true);
                }
                return m_instance;
            }
        }
    }
}
