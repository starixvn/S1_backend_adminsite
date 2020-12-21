import React, { useState, useEffect, useRef} from "react";
import ReactDOM from 'react-dom';
import WebAPI from "../../../module/WebAPI";
import { api } from '../../../module/WebAPI';
import qs from 'qs';

const initData = {
    optSite: null,
    option: null,
    checkwd: false,
    withdrawal: 1,
    file: '',
    walletType: null,
    productCategory: [],
    categoryId: null,
}
const defWalletType = {
	0: {
		value: 0,
		displayText: "Transfer"
	},
	1: {
		value: 1,
		displayText: "Seamless"
	}
};

const initWalletType = () => Object.values(defWalletType);

const RadioBox = ({Title, Options, Selected, OnChange, dataColumn, mainKey=null})=>{
    let opt = Options.map((o, oIdx)=>{
        let key = `${Title}-${o.value}${mainKey === null ? '': `-${mainKey}`}`;
        return <RadioBtn 
            key={key}
            Id={key}
            DisplayText={o.displayText}
            Value={`${o.value}${mainKey === null ? '': `-${mainKey}`}`} 
            dataColumn={dataColumn}
            Name={`${Title}${mainKey === null ? '': `-${mainKey}`}`}
            OnChange={OnChange.bind(this, o.value, dataColumn, mainKey, oIdx)}
            Checked={Selected === o.value} />
    });
    return(
        <>
            <div className="row-flex">
                <div className="input-item">
                    <label className="label">{Title}</label>
                </div>
            </div>
            <div className="row-flex">{opt}</div>
        </>
    )
}

const RadioBtn = ({Id, DisplayText, Value, Name, OnChange, Checked, Disabled})=>{
    return(
        <div className="custom-control custom-radio custom-control-inline">
            <input type="radio" id={Id} name={Name} className="custom-control-input revert-style" value={Value} onChange={OnChange} checked={Checked} disabled={Disabled}/>
            <label className="custom-control-label" htmlFor={Id}>{DisplayText}</label>
        </div>
    )
}

const App = () => {
    let _site = [];
    let headerRows = [];
    const [stateData, updateData] = useState({ ...initData });
    const [stateSend, updateSend] = useState(false);
    const EditFiled = (v, datacolumn) => {console.log(v,datacolumn); updateData({ ...stateData, [datacolumn]: v }); };
    const [WalletType,upwalletType] = useState(null);
    const [ProductCategory,upproductCategory] = useState([]);
    const [CategoryId,upcategoryId] = useState(null);
    const [TpId,upTpId] = useState(null);
    const [bodyRows,upbodyRows] = useState([]);
    const aa = useRef(null);
    const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
    });
    const GetPromoCategories = (isInit, walletType, value) => {
        if (walletType !== value) {
            let param = {
                Url: "BonusManage/GetPromoCategories",
                Data: qs.stringify({
                    WalletType: value
                })
            };
            WebAPI.PostData(param).then(res => {
                if (isMountedRef.current) {
                    if (res.data.errorCode === 0) {
                        upwalletType(value)
                        upproductCategory(GetproductCategory(res.data.data))
                    } else {
                        alert(res.data.message ?? "failed to get data.");
                    }
                }
            });
        }
    };
    const SelectTpId = (prop) => {
        upcategoryId(prop)
        upTpId(ProductCategory[0].tpId)
    };
    const GetproductCategory = (prop) => {
        let arr = prop.map(item => {
            return {
                value: item.id,
                displayText: item.name,
                tpId: item.tpId
            };
        });
        return arr;
    } 
    const SetMsg = () =>{
        let msg = "";
        let check = true;         
        if (CategoryId == null) { check = false; msg = "Please choose CategoryId.";}
        if (WalletType == null) { check = false; msg = "Please choose Wallet Type.";}
        if (!stateData.file) { check = false; msg = "Please choose excel file."; }  
        if (check && window.confirm("Are you sure to import data ?")) {
            let settings = { headers: { 'content-type': 'multipart/form-data' } }
            let file = document.getElementById("uploadfile").files[0];
            let formData = new FormData();
            formData.append("excelUpload", file, file.name);
            formData.append("TpId", parseInt(TpId));
            formData.append("CategoryId", parseInt(CategoryId));
            if (!stateSend) {
                api.post('/api/BonusManage/BonusImport', formData, settings)
                    .then(function (response) {
                        if (response.data) {
                            let result = [];
                            response.data.data.map((e,index) => {
                                if(e.status == 1){
                                    result.push(<tr key={index} bgcolor="#FFB5B5">
                                        <td>{e.siteId}</td>
                                        <td>{e.userName}</td>
                                        <td>{e.bonusAmount}</td>
                                        <td>{e.rollOver}</td>
                                        <td>{e.expiryDay}</td>
                                        <td>{e.memo}</td>
                                        <td>{e.status}</td>
                                        <td>{e.errorMessage}</td>
                                                </tr>)
                                }
                                else{
                                    result.push(<tr key={index}>
                                        <td>{e.siteId}</td>
                                        <td>{e.userName}</td>
                                        <td>{e.bonusAmount}</td>
                                        <td>{e.rollOver}</td>
                                        <td>{e.expiryDay}</td>
                                        <td>{e.memo}</td>
                                        <td>{e.status}</td>
                                        <td>{e.errorMessage}</td>
                                                </tr>)
                                }    
                            });
                            upbodyRows(result);                         
                        } else {
                            upbodyRows([]);
                            alert("sent error");
                        }
                    }).catch(function (error) {
 
                        alert(error);
                    })
            }
        }
        else {
            alert(msg);
        }
    }
    const titleData = [
        { dataColumn: "SiteId", columnTitle: "SiteId" },
        { dataColumn: "UserName", columnTitle: "UserName" },
        { dataColumn: "BonusAmount", columnTitle: "BonusAmount" },
        { dataColumn: "RollOver", columnTitle: "RollOver" },
        { dataColumn: "ExpiryDay", columnTitle: "ExpiryDay" },
        { dataColumn: "Memo", columnTitle: "Memo" },
        { dataColumn: "Status", columnTitle: "Status" },
        { dataColumn: "ErrorMessage", columnTitle: "ErrorMessage" }
    ];
    titleData.forEach((e, i) => {
		headerRows.push(<th key={`title-${i}`}>{e.columnTitle}</th>);
    });
    
    return(
        <>
            <div className="container-fluid">
                <h4 className="page-name">BonusImport</h4>
                <div className="form-group">
                    <label className="col-form-label d-block">Download Template File </label>
                    <a className="btn btn-danger" role="button" href="../../../resource/examplefile/BonusImport.xlsx" download="BonusImportTemplateFile.xlsx">Template File</a>
                </div>
                <div className="form-group width-200"> 
                    <label className="col-form-label">Upload File</label>
                    <input id="uploadfile" className="form-control" type="file" accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel" onChange={(e) => { EditFiled(e.target.value, 'file') }} disabled={stateSend} value={stateData.file} />
                </div>           
                <div>
                    <RadioBox
                        Title="Wallet Type"
                        Options={initWalletType()}
                        OnChange={GetPromoCategories.bind(
                     	    this,
                     	    false,
                     	    WalletType
                            )}
                        dataColumn="walletType"
                        Selected={WalletType}
                    />
                    {WalletType !== null ? (
									    <RadioBox
										    Title="Product Category"
										    Options={ProductCategory}
										    OnChange={SelectTpId}
										    dataColumn={"categoryId"
										    }
										    Selected={CategoryId
										    }
									    />
								            ) : null}
                </div>
                <div>
                    <button className="btn btn-add" style={{ marginTop: '20px', marginBottom: '40px' }} onClick={SetMsg}>Submit</button>
                </div>
            </div>

            <div className="table">
				<table className="table table-bordered">
					<thead>
						<tr>{headerRows}</tr>
					</thead>
					<tbody>{bodyRows}</tbody>
				</table>
			</div>           
        </>
    )
}


export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}