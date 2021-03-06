﻿#region Using References

using System;
using System.ComponentModel.DataAnnotations;

using CPT331.Web.Validation;

#endregion

namespace CPT331.Web.Models.Admin
{
    /// <summary>
    /// A representation of the Offence model that services the MVC framework. 
    /// </summary>
    public class StateModel : AdminModel
    {
        #region Constructors
        /// <summary>
        /// Creates an instance of StateModel using default values.
        /// </summary>
        public StateModel()
        {
        }

        /// <summary>
        /// Creates an instance of StateModel using the values provided.
        /// </summary>
        /// <param name="abbreviatedName">A two or three letter abbreviation of the state/territory.</param>
        /// <param name="name">The full name of the state/territory</param>
		public StateModel(string abbreviatedName, string name)
			: this(abbreviatedName, DateTime.UtcNow, DateTime.UtcNow, -1, false, true, name)
		{
		}

        /// <summary>
        /// Creates an instance of StateModel using the values provided.
        /// </summary>
        /// <param name="abbreviatedName">A two or three letter abbreviation of the state/territory.</param>
        /// <param name="dateCreatedUtc">The date when the record was created.</param>
        /// <param name="dateUpdatedUtc">The date when the record was last updated.</param>
        /// <param name="id">The unique ID value of the StateModel instance.</param>
        /// <param name="isDeleted">Specifies whether the instance is flagged as deleted.</param>
        /// <param name="isVisible">Specifies whether the instance is flagged as visible.</param>
        /// <param name="name">The full name of the state/territory</param>
		public StateModel(string abbreviatedName, DateTime dateCreatedUtc, DateTime dateUpdatedUtc, int id, bool isDeleted, bool isVisible, string name)
		{
			_abbreviatedName = abbreviatedName;
			_dateCreatedUtc = dateCreatedUtc;
			_dateUpdatedUtc = dateUpdatedUtc;
			_id = id;
			_isDeleted = isDeleted;
			_isVisible = isVisible;
			_name = name;
		}
        #endregion
        
        #region Instance Variables
        private string _abbreviatedName;
		private DateTime _dateCreatedUtc;
		private DateTime _dateUpdatedUtc;
		private int _id;
		private bool _isDeleted;
		private bool _isVisible;
		private string _name;
        #endregion

        #region Public Properties
        /// <summary>
        /// A two or three letter abbreviation of the state/territory.
        /// </summary>
        [DataType(DataType.Text)]
		[Display(Name = "Abbreviated Name")]
		[Required(ErrorMessage = "*")]
		[StringLength(3, ErrorMessage = "*")]
		public string AbbreviatedName
		{
			get
			{
				return _abbreviatedName;
			}
			set
			{
				_abbreviatedName = value;
			}
		}

        /// <summary>
        /// The date when the record was created.
        /// </summary>
        [DataType(DataType.DateTime)]
		[Display(Name = "Date Created")]
		[Required(ErrorMessage = "*")]
		public DateTime DateCreatedUtc
		{
			get
			{
				return _dateCreatedUtc;
			}
			set
			{
				_dateCreatedUtc = value;
			}
		}

        /// <summary>
        /// The date when the record was last updated.
        /// </summary>
        [DataType(DataType.DateTime)]
		[Display(Name = "Date Updated")]
		[Required(ErrorMessage = "*")]
		public DateTime DateUpdatedUtc
		{
			get
			{
				return _dateUpdatedUtc;
			}
			set
			{
				_dateUpdatedUtc = value;
			}
		}

        /// <summary>
        /// The unique ID value of the StateModel instance.
        /// </summary>
		[Integer(ErrorMessage = "*")]
		public int ID
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}

        /// <summary>
        /// Specifies whether the instance is flagged as deleted.
        /// </summary>
        [DataType(DataType.Text)]
		[Display(Name = "Deleted")]
		[Required(ErrorMessage = "*")]
		public bool IsDeleted
		{
			get
			{
				return _isDeleted;
			}
			set
			{
				_isDeleted = value;
			}
		}

        /// <summary>
        /// Specifies whether the instance is flagged as visible.
        /// </summary>
        [DataType(DataType.Text)]
		[Display(Name = "Visible")]
		[Required(ErrorMessage = "*")]
		public bool IsVisible
		{
			get
			{
				return _isVisible;
			}
			set
			{
				_isVisible = value;
			}
		}

        /// <summary>
        /// The full name of the state/territory.
        /// </summary>
        [DataType(DataType.Text)]
		[Display(Name = "Name")]
		[Required(ErrorMessage = "*")]
		[StringLength(100, ErrorMessage = "*")]
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}
        #endregion
    }
}