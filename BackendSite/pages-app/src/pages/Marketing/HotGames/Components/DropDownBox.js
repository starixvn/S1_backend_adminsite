import React from 'react'

export default function DropDownBox(props) {
    const onChange = (e) => {
        if (props.onChange) {
            props.onChange(e.target.value, props.dataColumn);
        }
    };
    let optionRows = props.options.map((item) => {
        return (
            <option key={`${props.columnTitle}-${item.value}`} value={item.value}>
                {item.displayText}
            </option>
        );
    });
    return (
        <div>
            <div className="input-item">
                <span className="label">{props.columnTitle}</span>
                <div className="field">
                    <select onChange={onChange} value={props.value} disabled={props.disabled}>
                        {optionRows}
                    </select>
                </div>
            </div>
        </div>
    );
};