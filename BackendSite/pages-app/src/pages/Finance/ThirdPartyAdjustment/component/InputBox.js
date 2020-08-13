import React from 'react';

export const InputField = (props) => {
    const EditField = (e) => {
        if (props.OnChange) {
            props.OnChange(e.target.value);
        } else if (props.OnBlur) {
            props.OnBlur(e.target.value);
        }
    }
    const handleKeyPress = (event) => {
        if (props.OnPressEnter && event.key === 'Enter') {
            props.OnPressEnter();
        }
    }
    return (
        <div className="form-group">
            <label className="col-form-label label">{props.columnTitle}</label>
            <input className="form-control width-200" type={props.IptType} defaultValue={props.defaultValue} value={props.value} onChange={EditField} onBlur={EditField} onKeyPress={handleKeyPress} disabled={props.Disabled} />
        </div>
    )
}