import React from "react";
import {number_format} from '../../../module/Formatting/NumberFormat';

export const InputBox = (props) => {
    return props.isEdit ? (
        <input type="number" defaultValue={props.defaultValue} onChange={(e) => {
            let value = e.target.value;
            if (!isNaN(value)) {
                props.onChange(e.target.value)
            }
        }} />
    ) : (
            number_format(props.defaultValue)
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