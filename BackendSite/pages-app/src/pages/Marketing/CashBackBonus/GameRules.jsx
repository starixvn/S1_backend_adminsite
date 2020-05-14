import React, { memo } from 'react';
import InputBox from './Component/InputBox';

const GameRules = memo(({SetGameRules, GameRuleDatas, TpList, SetDefaultPercentage, DefaultPercentage, isEdit, LockedGames, HandleLockGame})=>{    
    let disabled = !isEdit;
    return(
        <div className="filter">
            <div className="row-flex">                    
                <InputBox
                    Title={"Default Cashback Percentage"}
                    value={DefaultPercentage === null || DefaultPercentage === undefined ? '' : DefaultPercentage}
                    OnChange={SetDefaultPercentage}
                    type={'Number'}
                    Disabled={disabled}
                    iptSubClass='ratio'
                />
            </div>
            <GameList 
                Datas={GameRuleDatas}
                setData={SetGameRules}
                Disabled={disabled}
                tpList={TpList}
                LockedGames={LockedGames}
                HandleLockGame={HandleLockGame}
            />
        </div>
    )
})

const GameList = ({Datas, setData, Disabled, tpList, LockedGames, HandleLockGame})=>{
    let items = Datas && Object.keys(Datas).map(k=>{
        return(
            <ListItem
                key={`tp-gameRule-${k}`}
                tpName={tpList[k]}
                tpId={k}
                Data={Datas[k]}
                setData={setData}
                Disabled={Disabled}
                Locked={LockedGames[k]}
                HandleLockGame={HandleLockGame}
            />
        )
    })
    return(
        <div className="row-flex Cashback-disabled">
            <div className="container-fluid no-padding">
                <div className="list list--2nd is-open no-margin-buttom">
                    {items}
                </div>
            </div>
        </div>
    )
}

const ListItem = ({tpName, tpId, Data, setData, Disabled, HandleLockGame, Locked})=>{
    let _disabled = Disabled ? true : Locked;
    let switchKey = `switch-${tpId}`;
    return (
        <div className="list__item">
            <label className="checkbox checkbox-checked">
                <span className="text">{tpName}</span>
            </label>
            <div className="switch__container">
                <input id={switchKey} className="switch switch--flat" type="checkbox" disabled={Disabled} checked={!Locked} onClick={HandleLockGame.bind(null, tpId)}/>
                <label htmlFor={switchKey}></label>
            </div>
            <InputBox
                dataColumn={tpId}
                ClassName="input-item input-margin-left"
                HideTitle={true}
                value={Data === null || Data===undefined ? '' : Data}
                Disabled={_disabled}
                OnChange={setData}
                type={'Number'}
                iptSubClass='ratio'
            />
        </div>
    );
}

export default GameRules;