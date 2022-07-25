import React from 'react'

export default function DropDownBox(props) {
    const onChange = (e) => {
        if (props.onChange) {
            props.onChange(e.target.value, props.groupKey);
        }
    };
    let optionRows = props.options.map((item) => {
        return (
            <option key={`${props.groupKey}-${item.group}`} value={item.group}>
                {item.range}
            </option>
        );
    });
    return (
        <div className="input-item">
            <span className="label">{props.columnTitle}</span>
            <div className="field">
                <select onChange={onChange} value={props.value}>
                    {optionRows}
                </select>
            </div>
        </div>
    );
};