import React from "react";
import { OuterPanel, SwitchBtn, PanelClass } from './components';
import { ChannelInfoHeader, ChannelInfoContent } from './ChannelInfo';

export const PayInfoHeader = (props) => {
    return (
        <>
            <span style={{ color: 'white' }}>{props.Name}</span>
            {window.subFuncKeys.includes('f_pm_edit') ? <SwitchBtn Title="Status" checked={props.switchBtn.Status === 1} onChange={props.switchBtn.onChange} SwitchBtnKey={props.switchBtn.SwitchBtnKey} /> : null}
        </>
    );
}

export const PayInfoContent = (props) => {
    let channelList = props.ChannelList.map((channel, idx) => {
        let idxArr = [...props.idxArr, idx];
        return (
            <OuterPanel
                PanelClass={PanelClass.level2_mt05}
                HandleCollaps={props.HandleCollaps}
                idxArr={idxArr}
                isCollaps={channel.isCollaps}
                key={`channelList-${idx}`}
                Id={`${props.MainId}-channel-${idx}`}
                HeaderContent={
                    <ChannelInfoHeader
                        idxArr={idxArr}
                        Name={channel.channelName}
                        EditField={props.EditField}
                        Status={channel.editValue ? channel.editValue.status : channel.status}
                    />
                }
                Content={
                    <ChannelInfoContent
                        {...channel}
                        idxArr={idxArr}
                        ClickEdit={props.ClickEdit}
                        ClickCancel={props.ClickCancel}
                        ClickSave={props.ClickSave}
                        EditField={props.EditField}
                        ContentOnClick={props.ContentOnClick}
                    />
                }
            />
        );
    });

    return (
        <div className="table">
            {channelList}
        </div>
    )
}