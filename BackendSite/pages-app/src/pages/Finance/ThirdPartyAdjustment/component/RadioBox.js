import React from 'react';

export const RadioBtn = (props) => {
    return (
        <div className="custom-control custom-radio custom-control-inline">
            <input type="radio" id={props.Id} className="custom-control-input" value={props.Value} onChange={props.OnChange} checked={props.Checked} disabled={props.Disabled} />
            <label className="custom-control-label" htmlFor={props.Id}>{props.DisplayText}</label>
        </div>
    )
}