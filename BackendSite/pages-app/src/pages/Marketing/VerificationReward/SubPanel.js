import React, { useState, useEffect, useReducer } from "react";
import moment from "moment";
import { initVerificationType } from "./defCols";
import ActionButtons from "./Components/ActionButtons"
import RadioGroup from "./Components/RadioGroup"
import InputBox from "./Components/InputBox";
import StatusButton from "./Components/StatusButton";

function SubPanel ( { isEdit, reward, currencyTitle, switchStatus, clickEdit, clickCancel, clickSubmit} ) {
    return (
        <>
            <div className="panel level-1 mt-20">
                <a className="panel-title no-arrow" style={{ justifyContent: "space-between" }}>
                    <span style={{ color: "white" }}>{currencyTitle}</span>
                </a>
            </div>
            <div className="panel-body">
                <div className="filter">
                    <RadioGroup title="Verification Type" options={initVerificationType} disabled={!isEdit} toggle={() => {}} selected={reward.verificationType} />
                    <div className="row-flex">
                        <InputBox type="Number" value={reward.phoneReward} title={reward.verificationType === 0 ? "Phone Reward": "Phone & Email Reward"} disabled={!isEdit} dataColumn="123456789" />
                    </div>
                    {reward.verificationType === 0 ?
                    <div className="row-flex">
                        <InputBox type="Number" value={reward.emailReward} title="Email Reward" disabled={!isEdit} dataColumn="123456789" />
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
                                mainKey={`switch-${reward.siteId}-${reward.currencyId}`} 
                                checked={reward.status === 1} 
                                onChange={switchStatus.bind(this, reward.status)}
                            />
                    </div>
                </div>
                <ActionButtons 
                    isEdit={isEdit}
                    clickEdit={clickEdit}
                    clickCancel={clickCancel}
                    clickSubmit={clickSubmit}
                    disabled={false}
			    />
            </div>
        </>
    )
}

export default SubPanel