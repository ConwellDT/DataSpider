using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CFW.Web
{
    public class UserInfo
    {
        // Fields
        private string _LanguageType = string.Empty;
        private string _UserPlant = string.Empty;
        private string _UserGrp = string.Empty;
        private string _UserId = string.Empty;
        private string _UserName = string.Empty;
        private string _UserGrade = string.Empty;

        // Properties
        public string LanguageType
        {
            get
            {
                return this._LanguageType;
            }
            set
            {
                if ((value != null) && (value.Length > 0))
                {
                    this._LanguageType = value;
                }
            }
        }

        public string UserPlant
        {
            get
            {
                return this._UserPlant;
            }
            set
            {
                this._UserPlant = value;
            }
        }

        public string UserGrp
        {
            get
            {
                return this._UserGrp;
            }
            set
            {
                this._UserGrp = value;
            }
        }

        public string UserId
        {
            get
            {
                return this._UserId;
            }
            set
            {
                this._UserId = value;
            }
        }

        public string UserName
        {
            get
            {
                return this._UserName;
            }
            set
            {
                this._UserName = value;
            }
        }

        public string UserGrade
        {
            get
            {
                return this._UserGrade;
            }
            set
            {
                this._UserGrade = value;
            }
        }
    }
}
