import React from 'react';

const ActionButtons = (props) => {
    return props.isEdit ? (
        <div className="button-action">
            <button
                className="btn btn-cancel"
                style={{ marginRight: "5px" }}
                onClick={props.clickCancel}
                disabled={props.disabled}>
                Cancel
            </button>
            <button
                className="btn btn-approved"
                style={{ marginRight: "5px" }}
                onClick={props.clickSubmit}
                disabled={props.disabled}>
                Submit
            </button>
        </div>
        ) : (
        <div className="button-action">
            <button
                className="btn btn-verify"
                style={{ marginRight: "5px" }}
                onClick={props.clickEdit}
                disabled={props.disabled}>
                Edit
            </button>
        </div >
    )
}

export default ActionButtons;



