import React, { useState } from "react";

export const InputBox = (props) => {
    return props.isEdit ? (
        <input type="number" defaultValue={props.defaultValue} onChange={(e) => {
            let value = e.target.value;
            if (!isNaN(value)) {
                props.onChange(e.target.value)
            }
        }} />
    ) : (
            props.defaultValue
        )
}

export const ActionButtons = (props) => {
    return props.isEdit ? (
        <div className="button-action">
            <button className="btn btn-save" onClick={props.HandleUpdate}>
                <i className="icon-save"></i>
            </button>
            <button className="btn btn-icon-cancel" onClick={props.ClickEdit}>
                <i className="icon-cancel"></i>
            </button>
        </div>
    ) : (
            <div className="button-action">
                <button className="btn btn-edit" onClick={props.ClickEdit}>
                    <i className="icon-edit"></i>
                </button>
                {props.isDelete ? (
                    <button className="btn btn-icon-delete" onClick={props.HandleDelete}>
                        <i className="icon-delete"></i>
                    </button>
                ) : (null)
                }
            </div >
        )
}

export const SwitchBtn = (props) => {
    return props.isEdit ?
        props.Title ? (
            <div style={{ marginRight: "25px" }}>
                <span style={{ padding: "0 10px", color: 'white' }}>{props.Title}:</span>
                <div className="switch__container">
                    <input id={props.SwitchBtnKey} className="switch switch--flat" type="checkbox" checked={props.checked} onChange={(e) => { props.onChange(e.target.checked); }} />
                    <label htmlFor={props.SwitchBtnKey}></label>
                </div>
            </div>
        ) : (
                <div className="switch__container">
                    <input id={props.SwitchBtnKey} className="switch switch--flat" type="checkbox" checked={props.checked} onChange={(e) => { props.onChange(e.target.checked); }} />
                    <label htmlFor={props.SwitchBtnKey}></label>
                </div>
            ) : (props.checked) ? "V" : "X"
}