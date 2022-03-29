import React from 'react';

export const SelectList = (props) => {
    let selectOptions = props.SelectOptions.map((item) => {
        return <option value={item.value}>{item.name}</option>;
    });

    return (
        <div className="form-group width-200">
            <span className="label">{props.DisplayText}</span>
            <div className="field">
                
                <select className="form-control" value={props.Value} onChange={(e) => { props.OnChange(e.target.value)}} disabled={props.Disabled}>
                    {selectOptions}
                </select>
            </div>
        </div>
    )
}