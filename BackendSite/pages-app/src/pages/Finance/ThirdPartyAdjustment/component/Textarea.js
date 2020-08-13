import React from 'react';

export const TextArea = (props) => {
    return (
        <div className="form-group width-200">
            <span className="label">{props.DisplayText}</span>
            <div className="field">
                <textarea className="form-control" value={props.value} onChange={(e) => { props.OnBlur(e.target.value) }} disabled={props.Disabled} defaultValue={props.defaultValue} />
            </div>
        </div>
    )
}