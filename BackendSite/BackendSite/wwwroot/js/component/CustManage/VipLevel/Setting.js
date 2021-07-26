import { number_format } from '../../commJS/common';
import moment from "moment-timezone";
const Setting = (props) => {

    let levelSetting = null;
    let levelSettingArr = {};
    if (props.Setting) {
        levelSetting = Object.values(props.Setting).map(i => {
            return (<option key={i.level} value={i.level}>{i.levelName}</option>);
        })

        Object.values(props.Setting).map(i => {
            levelSettingArr[i.level] = i.levelName;
        })
    }

    const selectChange = (e) => {
        props.onChange(e.target.value, "vipLevel");
    }

    return (
        <table className="table table-responsive-md">
            <thead>
                <tr>
                    <th scope="col">Manually</th>
                    <th scope="col">Level</th>
                    <th scope="col">Qualified Time(Adjusted to GMT+8)</th>
                    <th scope="col">Action</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    {
                        props.isEdit && props.UpdateDatas ? (
                            <>
                                <td></td>
                                <td>
                                    <div className="filter input-item">
                                        <select className="field" value={number_format(props.UpdateDatas.vipLevel)} onChange={selectChange} >
                                            <option value="0">None</option>
                                            {levelSetting}
                                        </select>
                                    </div>
                                </td>
                                <td></td>
                                <td>
                                    <div className="button-action">
                                        <button className="btn btn-save" onClick={props.editVIPUpdate}>
                                            <i className="icon-save"></i>
                                        </button>
                                        <button className="btn btn-icon-cancel" onClick={props.onClickEdit.bind(null, props.isEdit)}>
                                            <i className="icon-cancel"></i>
                                        </button>
                                    </div>
                                </td>
                            </>
                        ) : (
                                <>
                                    <td>
                                        {
                                            subFuncKeys.includes('ca_vipLevelEdit') ?
                                                <div className="">
                                                    <input id="switch-manually" className="switch switch--flat" type="checkbox" checked={props.Data.manually} onChange={props.editManuallyUpdate.bind(null, !props.Data.manually)} />
                                                    <label htmlFor="switch-manually"></label>
                                                </div>
                                                :
                                                (props.Data.manually) ? "Yes" : "No"
                                        }
                                    </td>
                                    <td>{levelSettingArr[number_format(props.Data.vipLevel)]}</td>
                                    <td>{moment(props.Data.qualifiedTime).format('l LTS')}</td>
                                    <td>
                                        {
                                            subFuncKeys.includes('ca_vipLevelEdit') ?
                                                <div className="button-action">
                                                    <button className="btn btn-edit" onClick={props.onClickEdit.bind(null, props.isEdit)}>
                                                        <i className="icon-edit"></i>
                                                    </button>
                                                </div>
                                                : null
                                        }
                                    </td>
                                </>
                            )
                    }
                </tr>
            </tbody>
        </table>
    )
}


export default Setting;