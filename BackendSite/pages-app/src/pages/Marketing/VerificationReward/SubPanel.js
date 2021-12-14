import React, { useState, useEffect, useReducer } from "react";
import moment from "moment";
import { initVerificationType } from "./defCols";
import TabButtonGroup from "./Components/TabButtonGroup"
import RadioGroup from "./Components/RadioGroup"
import InputBox from "./Components/InputBox";
import StatusButton from "./Components/StatusButton";

function SubPanel ( {reward, } ) {
    return (
        <>
            <div className="panel-body">
                <div className="filter">
                    <RadioGroup title="Verification Type" options={initVerificationType} toggle={() => {}} selected={reward.verificationType} />
                    <div className="row-flex">
                        <InputBox type="Number" value={reward.phoneReward} title={reward.verificationType === 0 ? "Phone Reward": "Phone & Email Reward"} dataColumn="123456789" />
                    </div>
                    {reward.verificationType === 0 ?
                    <div className="row-flex">
                        <InputBox type="Number" value={reward.emailReward} title="Email Reward" dataColumn="123456789" />
                    </div> : null}

                    <div className="row-flex">
                        <label className="label">LastUpdateUser: {reward.lastUpdateUser?? "-"}</label>
                    </div>
                    <div className="row-flex">
                        <label className="label">LastUpdateTime: {reward.lastUpdateTime ? moment(reward.lastUpdateTime).format("l LTS") : "-"}</label>
                    </div>
                    <div className="row-flex">
                        <label className="label">Status:</label>
                            <StatusButton 
                                mainKey={`switch-${reward.siteId}`} 
                                checked={reward.status === 1} 
                                onChange={()=>{}}
                            />
                    </div>
                </div>
            </div>
        </>
    )
}

export default SubPanel