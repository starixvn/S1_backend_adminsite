import React,{useState, useEffect} from 'react';
import WebAPI from '../../../module/WebAPI';
import moment from 'moment';
import Edit from './Edit';

const SpecialItemType = {
	actionBtn: "actionBtn"
};

const _titleData = () => {
    return [
        { dataColumn:"bonusCode", columnTitle: "BonusCode"},
        { dataColumn:"promotionType", columnTitle:"PromotionType" },
        { dataColumn:"categoryId", columnTitle: "CategoryId"},
        { dataColumn:"periodFrom", columnTitle: "PeriodFrom", body: { format: dateFormat }},
        { dataColumn:"periodTo", columnTitle: "PeriodTo", body: { format: dateFormat }},
        { dataColumn:"sort", columnTitle: "Sort"},
        { dataColumn:"status", columnTitle: "Status", body: { format: statusFomat }},
        { dataColumn:"edit", columnTitle:"Edit",  body: { SpecialItem: SpecialItemType.actionBtn }}, 
    ]
}

export default (props) => {
    const titleData = _titleData();
    const [isEdit, setIsEdit] = useState(false);
    const [data, setData] = useState([]);
    const [stateData, updateData] = useState({});
    const [siteList, setSiteList] = useState([]);

    useEffect(() => {
        let param = {
            Url: 'BonusManage/GetPromotionList',
        };
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setData(res.data.data)
            }else{
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    },[])

    const clickCreateBonus = () => {
        let param = {
            Url: 'BonusManage/GetSiteListByLoginUser',
        };
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setSiteList(res.data.data)
                setIsEdit(true);
            }else{
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    const clickEdit = (idx) => {
        let param = {
            Url: 'BonusManage/GetPromotion',
            bonusCode: data[idx].bonusCode
        };
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                updateData(res.data.data);
                setIsEdit(true);
            }else{
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    
    }

    let headerRows = [];
    titleData.forEach((e, i)=>{
        headerRows.push(
            <th key={`title-${i}`}>{e.columnTitle}</th>
        )
    })

    let bodyRows = data.map((_data, d_idx) => {
        let arr=titleData.map((element, t_idx)=>{
            let body_content = null;
            if (element.body && element.body.SpecialItem) {
                if (element.body.SpecialItem === SpecialItemType.actionBtn) {
                    body_content = (
                        <ActionButtons clickEdit={clickEdit.bind(null, d_idx)}/>
                    )
                }
            } else{
                body_content = element.body && element.body.format ? element.body.format(_data[element.dataColumn]) : _data[element.dataColumn]
            }

            return(
                <td key={`td-${t_idx}`}>
                    {body_content}
                </td> 
            )
        })

        return(
            <tr key={`body-${d_idx}`}>{arr}</tr>
        )
    })

    const DataColumn = titleData.map(e => {
        return e.dataColumn
    })

    return isEdit ? <Edit Datas={stateData} SiteList={siteList}/> : (
        <div className="table">
            <div className="table-options">
                <button type="button" className="btn btn-approved" onClick={clickCreateBonus}>Create Bonus</button>
            </div>
            <table className="table table-striped">
                <thead>
                    <tr>{headerRows}</tr>
                </thead>
                <tbody>{bodyRows}</tbody>
            </table>
        </div>
    )
}

const statusFomat = (checked, subData) => {    
    return(
        <div className="switch__container">
            <input id="switch-bonus" className="switch switch--flat" type="checkbox" checked={checked} onChange={()=>null}/>
            <label htmlFor="switch-bonus"></label>
        </div>
    )
}

const dateFormat = (createTime) => {
    return moment(createTime).format('l LTS');
}

const ActionButtons = (props) => {
    return(
        <div className="button-action" onClick={props.clickEdit}>
            <button className="btn btn-edit">
                <i className="icon icon-mode_edit"></i>
            </button>    
        </div>
    )
}