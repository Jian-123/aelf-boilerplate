import React, { Component } from 'react';
import { Link } from 'react-router';  // 路由
import { Menu, Icon } from 'antd';  // Menu 导航菜单 Icon 图标
import '../styles/home.less';

// 左侧菜单栏
const SubMenu = Menu.SubMenu;

class Home extends Component {
    render () {
        const {children} = this.props;
        return (
            <div>
                <header className="header">
                    <Link to="/">ReactManager</Link>
                </header>

                <main className="main">
                    <div className="menu">
                        <Menu mode="inline" theme="dark" style={{width: '240'}}>
                            <SubMenu key="user" title={<span><Icon type="user"/><span>用户管理</span></span>}>
                                <Menu.Item key="user-list">
                                    <Link to="/user/list">用户列表</Link>
                                </Menu.Item>
                                <Menu.Item key="user-add">
                                    <Link to="/user/add">添加用户</Link>
                                </Menu.Item>
                            </SubMenu>

                            <SubMenu key="book" title={<span><Icon type="book"/><span>图书管理</span></span>}>
                                <Menu.Item key="book-list">
                                    <Link to="/book/list">图书列表</Link>
                                </Menu.Item>
                                <Menu.Item key="book-add">
                                    <Link to="/book/add">添加图书</Link>
                                </Menu.Item>
                            </SubMenu>
                        </Menu>
                    </div>

                    <div className="content">
                        {children}
                    </div>
                </main>
            </div>
        );
    }
}


export default Home;