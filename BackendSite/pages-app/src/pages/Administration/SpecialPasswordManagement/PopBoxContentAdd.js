import React, { useState } from "react";
import { api } from '../../../module/WebAPI';

export const PopBoxContentAdd = (props) => {
    const [AdjustmentMax, setAdjustmentMax] = useState(0);

    return (
        <div>
            <div>
                <h1>Add Adjustment Limit</h1>
                <form className="ui form">
                    <div className="field">
                        <label>Password</label>
                        <input type="number" defaultValue={AdjustmentMax} onChange={(e) => { setAdjustmentMax(parseFloat(e.target.value)) }} />
                        <label>(0 = Unlimited)</label>
                    </div>
                    <button className="ui right floated primary button" onClick={handleSave} >Save</button>
                    <button className="ui right floated button" onClick={props.ClickCancel}>Cancel</button>
                    <div className="clearfix"></div>
                </form>
            </div>
        </div>
    )
}