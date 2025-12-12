using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GigNovaModels
{
    public abstract class Model : INotifyDataErrorInfo
    {

        private Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        private object threadLock = new object();
        private bool isValid;

        public Model()
        {
        }

        public bool HasErrors
        {
            get { return errors.Any(propErrors => propErrors.Value != null && propErrors.Value.Count > 0); }
        }

        public bool IsValid
        {
            get { return !this.HasErrors; }
            
        }

        public void OnErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
            {
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
                
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                if (errors.ContainsKey(propertyName) && (errors[propertyName] != null) && errors[propertyName].Count > 0)
                    return errors[propertyName].ToList();
                else
                    return null;
            }
            else
                return errors.SelectMany(err => err.Value.ToList());
        }

        public Dictionary<string, List<string>> AllErrors()
        {
            return this.errors;
        }

        public void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            lock (threadLock)
            {
                var validationContext = new ValidationContext(this, null, null);
                validationContext.MemberName = propertyName;
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateProperty(value, validationContext, validationResults);
               //clear previous errors from tested property
                if (errors.ContainsKey(propertyName))
                    errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
                HandleValidationResults(validationResults);
            }
        }

        public void Validate()
        {
            this.isValid = true; //reset IsValid before validation
            lock (threadLock)
            {
                var validationContext = new ValidationContext(this, null, null);
                var validationResults = new List<ValidationResult>();
                Validator.TryValidateObject(this, validationContext, validationResults, true);

                //clear all previous errors
                var propNames = errors.Keys.ToList();
                errors.Clear();
                HandleValidationResults(validationResults);
                this.isValid = validationResults.Count == 0;
            }
        }

        private void HandleValidationResults(List<ValidationResult> validationResults)
        {
            //Group validation results by property names
            var resultsByPropNames = from res in validationResults
                                     from mname in res.MemberNames
                                     group res by mname into g
                                     select g;

            //add errors to dictionary and inform  binding engine about errors
            foreach (var prop in resultsByPropNames)
            {
                var messages = prop.Select(r => r.ErrorMessage).ToList();
                errors.Add(prop.Key, messages);
                OnErrorsChanged(prop.Key);
            }
        }
    }


}
