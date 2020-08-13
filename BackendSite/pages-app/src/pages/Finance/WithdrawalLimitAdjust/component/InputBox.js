import React from 'react';

export const InputField = (props) => {
    const EditField = (e) => {
        if (props.onChange) {
            props.onChange(e.target.value, props.dataColumn);
        } else if (props.onBlur) {
            props.onBlur(e.target.value, props.dataColumn);
        }
    }
    const handleKeyPress = (event) => {
        if (props.OnPressEnter && event.key === 'Enter') {
            props.OnPressEnter();
        }
    }
    return (
        <div className="input-item">
            <span className="label">{props.columnTitle}</span>
            <input className="form-control" type={props.IptType} defaultValue={props.defaultValue} value={props.Value} onChange={EditField} onBlur={EditField} onKeyPress={handleKeyPress} />
        </div>
    )
}